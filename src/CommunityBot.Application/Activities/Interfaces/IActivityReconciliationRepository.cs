using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities.Interfaces;

public interface IActivityReconciliationRepository
{
    void Add(ActivityReconciliation reconciliation);

    Task<ActivityReconciliation?> GetNextForUpdateAsync(CancellationToken ct = default);

    void Remove(ActivityReconciliation reconciliation);
}