namespace CommunityBot.Application.Activities.Interfaces;

/// <summary>
/// Reconciles conservatively captured activities against the subscriptions that covered their occurrence time.
/// </summary>
public interface IActivityReconciliationService
{
    /// <summary>
    /// Reconciles at most one pending activity.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a reconciliation was completed;
    /// otherwise <see langword="false"/> when no work was available.
    /// </returns>
    Task<bool> ReconcileNextAsync(CancellationToken ct = default);
}