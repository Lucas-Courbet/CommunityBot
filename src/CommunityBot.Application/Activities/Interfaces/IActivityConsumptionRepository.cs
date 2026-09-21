using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities.Interfaces;

/// <summary>
/// Provides persistence operations required to register and process durable activity consumptions.
/// </summary>
public interface IActivityConsumptionRepository
{
    void AddRange(IEnumerable<ActivityConsumption> consumptions);

    /// <summary>
    /// Acquires the oldest eligible pending consumption for exclusive processing.
    /// </summary>
    /// <remarks>
    /// Consumptions whose retry delay has not elapsed are excluded. Already locked rows are skipped
    /// so concurrent workers can claim different consumptions. The returned entity includes its
    /// associated activity event and subscription.
    /// </remarks>
    Task<ActivityConsumption?> GetNextPendingForUpdateAsync(DateTime now, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a consumption while acquiring a row lock for the current transaction.
    /// </summary>
    Task<ActivityConsumption?> GetByIdForUpdateAsync(long id, CancellationToken ct = default);
}