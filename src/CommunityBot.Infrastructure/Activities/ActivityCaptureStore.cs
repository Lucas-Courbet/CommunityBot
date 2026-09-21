using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Core.Activities;
using CommunityBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommunityBot.Infrastructure.Activities;

/// <summary>
/// Provides the persistence operations required by the activity capture workflow.
/// </summary>
/// <remarks>
/// Transaction and savepoint ownership remain the responsibility of
/// <see cref="ActivityCaptureTransactionCoordinator"/>. Discard operations restore EF Core tracking
/// state after a database savepoint rollback, which does not revert the change tracker itself.
/// </remarks>
public sealed class ActivityCaptureStore(
    AppDbContext context,
    IActivityCaptureGateRepository captureGateRepository,
    IActivitySubscriptionRepository subscriptionRepository,
    IActivityEventRepository activityEventRepository,
    IActivityConsumptionRepository consumptionRepository,
    IActivityCaptureIncidentRepository incidentRepository,
    IActivityReconciliationRepository reconciliationRepository)
{
    /// <summary>
    /// Locks the permanent capture gate before resolving subscriptions matching the occurrence time.
    /// </summary>
    public async Task<IReadOnlyList<ActivitySubscription>> ResolveMatchingSubscriptionsAsync(
        ActivityEventType eventType,
        DateTime occurredAt,
        CancellationToken ct = default)
    {
        var gate = await captureGateRepository.GetForUpdateAsync(eventType, ct);

        if (gate is null)
            throw new MissingActivityCaptureGateException(eventType);

        return await subscriptionRepository.GetMatchingAsync(eventType, occurredAt, ct);
    }

    public async Task PersistCaptureAsync(
        ActivityEvent activityEvent,
        IReadOnlyList<ActivityConsumption> consumptions,
        CancellationToken ct = default)
    {
        activityEventRepository.Add(activityEvent);
        consumptionRepository.AddRange(consumptions);

        await context.SaveChangesAsync(ct);
    }

    public async Task PersistConservativeCaptureAsync(
        ActivityEvent activityEvent,
        ActivityReconciliation reconciliation,
        CancellationToken ct = default)
    {
        activityEventRepository.Add(activityEvent);
        reconciliationRepository.Add(reconciliation);

        await context.SaveChangesAsync(ct);
    }

    public async Task PersistIncidentAsync(
        ActivityCaptureIncident incident,
        CancellationToken ct = default)
    {
        incidentRepository.Add(incident);
        await context.SaveChangesAsync(ct);
    }

    public void DiscardCapture(
        ActivityEvent activityEvent,
        IReadOnlyList<ActivityConsumption> consumptions)
    {
        foreach (var consumption in consumptions)
            context.Entry(consumption).State = EntityState.Detached;

        context.Entry(activityEvent).State = EntityState.Detached;
    }

    public void DiscardConservativeCapture(
        ActivityEvent activityEvent,
        ActivityReconciliation reconciliation)
    {
        context.Entry(reconciliation).State = EntityState.Detached;
        context.Entry(activityEvent).State = EntityState.Detached;
    }

    public void DiscardIncident(ActivityCaptureIncident incident)
    {
        context.Entry(incident).State = EntityState.Detached;
    }
}