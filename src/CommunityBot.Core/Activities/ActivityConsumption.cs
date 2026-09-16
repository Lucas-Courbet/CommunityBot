using CommunityBot.Core.Common;

namespace CommunityBot.Core.Activities;

/// <summary>
/// Tracks durable processing of one activity event by one subscription.
/// </summary>
public sealed class ActivityConsumption : IEntity<long>, IAuditable
{
    private ActivityConsumption()
    {
    }

    public ActivityConsumption(ActivityEvent activityEvent, ActivitySubscription subscription)
    {
        ActivityEvent = activityEvent ?? throw new ArgumentNullException(nameof(activityEvent));
        Subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));
        Status = ActivityConsumptionStatus.Pending;
    }

    public long Id { get; private set; }

    public long ActivityEventId { get; private set; }

    public ActivityEvent ActivityEvent { get; private set; } = null!;

    public long SubscriptionId { get; private set; }

    public ActivitySubscription Subscription { get; private set; } = null!;

    public ActivityConsumptionStatus Status { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTime? LastAttemptAt { get; private set; }

    public string? LastError { get; private set; }

    public DateTime? NextAttemptAt { get; private set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public void MarkProcessed(DateTime attemptedAt)
    {
        EnsurePending();
        ValidateUtc(attemptedAt);

        RecordAttempt(attemptedAt);
        Status = ActivityConsumptionStatus.Processed;
        NextAttemptAt = null;
    }

    public void MarkIgnored(DateTime attemptedAt)
    {
        EnsurePending();
        ValidateUtc(attemptedAt);

        RecordAttempt(attemptedAt);
        Status = ActivityConsumptionStatus.Ignored;
        NextAttemptAt = null;
    }

    public void ScheduleRetry(DateTime attemptedAt, string error, DateTime nextAttemptAt)
    {
        EnsurePending();
        ValidateUtc(attemptedAt);
        ValidateUtc(nextAttemptAt);
        ValidateError(error);

        if (nextAttemptAt <= attemptedAt)
            throw new ArgumentOutOfRangeException(
                nameof(nextAttemptAt),
                "Next attempt must be later than the failed attempt.");

        RecordAttempt(attemptedAt);
        LastError = error;
        NextAttemptAt = nextAttemptAt;
    }

    public void MarkError(DateTime attemptedAt, string error)
    {
        EnsurePending();
        ValidateUtc(attemptedAt);
        ValidateError(error);

        RecordAttempt(attemptedAt);
        LastError = error;
        NextAttemptAt = null;
        Status = ActivityConsumptionStatus.Error;
    }

    private void RecordAttempt(DateTime attemptedAt)
    {
        AttemptCount++;
        LastAttemptAt = attemptedAt;
    }

    private void EnsurePending()
    {
        if (Status != ActivityConsumptionStatus.Pending)
            throw new InvalidOperationException(
                $"Only a pending activity consumption can be processed. Current status is '{Status}'.");
    }

    private static void ValidateUtc(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Activity timestamps must use UTC.");
    }

    private static void ValidateError(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Processing error cannot be blank.", nameof(error));
    }
}