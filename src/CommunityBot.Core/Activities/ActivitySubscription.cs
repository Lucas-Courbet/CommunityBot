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

    /// <summary>
    /// Stable consumer-specific reference identifying the subscribed context.
    /// </summary>
    public required string ContextReference { get; init; }

    /// <summary>
    /// Inclusive beginning of the capture window.
    /// </summary>
    public required DateTime CaptureFrom { get; init; }

    /// <summary>
    /// Exclusive end of the capture window.
    /// </summary>
    public required DateTime CaptureUntil { get; set; }

    public ActivityCaptureGate? CaptureGate { get; set; }

    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Stops future capture at the specified instant without extending an already earlier deadline.
    /// </summary>
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

    /// <summary>
    /// Extends the capture window to the supplied later deadline.
    /// </summary>
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