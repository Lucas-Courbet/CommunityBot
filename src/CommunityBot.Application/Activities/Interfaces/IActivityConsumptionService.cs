namespace CommunityBot.Application.Activities.Interfaces;

/// <summary>
/// Processes durable activity consumptions.
/// </summary>
public interface IActivityConsumptionService
{
    /// <summary>
    /// Processes at most one eligible pending consumption.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a consumption was processed;
    /// otherwise <see langword="false"/> when no work was available.
    /// </returns>
    Task<bool> ProcessNextAsync(CancellationToken ct = default);
}