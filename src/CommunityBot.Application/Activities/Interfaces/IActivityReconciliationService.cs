namespace CommunityBot.Application.Activities.Interfaces;

public interface IActivityReconciliationService
{
    Task<bool> ReconcileNextAsync(CancellationToken ct = default);
}