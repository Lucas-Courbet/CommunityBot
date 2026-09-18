using CommunityBot.Core.Goals;

namespace CommunityBot.Application.Goals;

public interface ICommunityGoalService
{
    Task<CommunityGoal> CreateAsync(CommunityGoalCreateRequest request, CancellationToken ct = default);
}