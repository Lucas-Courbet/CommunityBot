namespace CommunityBot.Application.Activities;

public sealed class ActivityConsumptionProcessingException(
    long consumptionId,
    DateTime attemptedAt,
    Exception innerException)
    : Exception($"Activity consumption {consumptionId} failed during processing.", innerException)
{
    public long ConsumptionId { get; } = consumptionId;

    public DateTime AttemptedAt { get; } = attemptedAt;
}