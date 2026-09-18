using CommunityBot.Application.Common.Persistence;
using CommunityBot.Core.Goals;

namespace CommunityBot.Application.Goals;

public interface ICommunityGoalRepository : IBaseRepository<CommunityGoal, string>
{
    Task<CommunityGoal?> GetByIdForUpdateAsync(string goalId, CancellationToken ct = default);
}