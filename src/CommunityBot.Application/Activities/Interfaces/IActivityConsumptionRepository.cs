using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities.Interfaces;

public interface IActivityConsumptionRepository
{
    void AddRange(IEnumerable<ActivityConsumption> consumptions);

    Task<ActivityConsumption?> GetNextPendingForUpdateAsync(DateTime now, CancellationToken ct = default);

    Task<ActivityConsumption?> GetByIdForUpdateAsync(long id, CancellationToken ct = default);
}