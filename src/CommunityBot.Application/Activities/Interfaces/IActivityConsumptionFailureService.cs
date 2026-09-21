namespace CommunityBot.Application.Activities.Interfaces;

/// <summary>
/// Persists the durable outcome of a failed activity consumption attempt.
/// </summary>
public interface IActivityConsumptionFailureService
{
    /// <summary>
    /// Records the failed attempt in a transaction independent from the failed processing transaction.
    /// </summary>
    Task RecordFailureAsync(
        ActivityConsumptionProcessingException failure,
        CancellationToken ct = default);
}