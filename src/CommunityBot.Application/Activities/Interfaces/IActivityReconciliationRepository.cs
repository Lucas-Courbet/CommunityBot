using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities.Interfaces;

/// <summary>
/// Provides persistence operations for pending activity reconciliations.
/// </summary>
public interface IActivityReconciliationRepository
{
    void Add(ActivityReconciliation reconciliation);

    /// <summary>
    /// Acquires the oldest available pending reconciliation for exclusive processing.
    /// </summary>
    /// <remarks>
    /// Already locked rows are skipped so concurrent workers can claim different work items.
    /// The returned reconciliation includes its associated activity event.
    /// </remarks>
    Task<ActivityReconciliation?> GetNextForUpdateAsync(CancellationToken ct = default);

    void Remove(ActivityReconciliation reconciliation);
}