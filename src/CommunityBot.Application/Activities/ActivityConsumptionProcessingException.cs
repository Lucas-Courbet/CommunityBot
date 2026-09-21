namespace CommunityBot.Application.Activities;

/// <summary>
/// Represents a failure that occurred after an activity consumption was claimed for processing.
/// </summary>
public sealed class ActivityConsumptionProcessingException(
    long consumptionId,
    DateTime attemptedAt,
    Exception innerException)
    : Exception($"Activity consumption {consumptionId} failed during processing.", innerException)
{
    public long ConsumptionId { get; } = consumptionId;

    /// <summary>
    /// UTC timestamp of the processing attempt that failed.
    /// </summary>
    public DateTime AttemptedAt { get; } = attemptedAt;
}