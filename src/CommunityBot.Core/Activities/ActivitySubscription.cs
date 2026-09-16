using CommunityBot.Core.Common;

namespace CommunityBot.Core.Activities;

/// <summary>
/// Defines a durable consumer subscription to one activity type for a bounded time window.
/// </summary>
public sealed class ActivitySubscription : IEntity<long>, IAuditable
{
    public long Id { get; init; }

    public required ActivityEventType EventType { get; init; }

    public required ActivityConsumerType ConsumerType { get; init; }

    public required string ContextReference { get; init; }

    public required DateTime CaptureFrom { get; init; }

    public required DateTime CaptureUntil { get; set; }

    public ActivityCaptureGate? CaptureGate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public void CloseAt(DateTime closedAt)
    {
        ValidateUtc(closedAt, nameof(closedAt));

        if (closedAt <= CaptureFrom)
            throw new ArgumentOutOfRangeException(
                nameof(closedAt),
                "Capture closure must be later than the capture start.");

        if (closedAt < CaptureUntil)
            CaptureUntil = closedAt;
    }

    public void ExtendUntil(DateTime captureUntil)
    {
        ValidateUtc(captureUntil, nameof(captureUntil));

        if (captureUntil < CaptureUntil)
            throw new ArgumentOutOfRangeException(
                nameof(captureUntil),
                "An extended capture deadline cannot be earlier than the current deadline.");

        CaptureUntil = captureUntil;
    }

    private static void ValidateUtc(DateTime value, string paramName)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Activity timestamps must use UTC.", paramName);
    }
}