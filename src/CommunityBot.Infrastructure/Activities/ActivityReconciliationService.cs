using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Application.Common.Persistence;
using CommunityBot.Core.Activities;

namespace CommunityBot.Infrastructure.Activities;

/// <summary>
/// Reconciles conservatively captured activities against subscriptions that covered their occurrence time.
/// </summary>
/// <remarks>
/// Each reconciliation owns a short transaction. The pending marker is claimed exclusively, then the
/// event-type capture gate is locked before historical subscriptions are resolved. Resulting consumptions
/// and removal of the reconciliation marker are committed atomically, so a failed transaction leaves the
/// durable marker available for a later attempt.
/// </remarks>
public sealed class ActivityReconciliationService(
    IPersistenceContext persistenceContext,
    IActivityReconciliationRepository reconciliationRepository,
    IActivityCaptureGateRepository captureGateRepository,
    IActivitySubscriptionRepository subscriptionRepository,
    IActivityConsumptionRepository consumptionRepository)
    : IActivityReconciliationService
{
    /// <inheritdoc />
    public async Task<bool> ReconcileNextAsync(CancellationToken ct = default)
    {
        await using var transaction = await persistenceContext.BeginTransactionAsync(ct);

        var reconciliation = await reconciliationRepository.GetNextForUpdateAsync(ct);

        if (reconciliation is null)
        {
            await transaction.CommitAsync(ct);
            return false;
        }

        var activityEvent = reconciliation.ActivityEvent;

        var gate = await captureGateRepository.GetForUpdateAsync(activityEvent.EventType, ct);

        if (gate is null)
            throw new MissingActivityCaptureGateException(activityEvent.EventType);

        var subscriptions = await subscriptionRepository.GetMatchingAsync(
            activityEvent.EventType,
            activityEvent.OccurredAt,
            ct);

        if (subscriptions.Count > 0)
        {
            var consumptions = subscriptions
                .Select(subscription => new ActivityConsumption(activityEvent, subscription))
                .ToList();

            consumptionRepository.AddRange(consumptions);
        }

        reconciliationRepository.Remove(reconciliation);

        await persistenceContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return true;
    }
}