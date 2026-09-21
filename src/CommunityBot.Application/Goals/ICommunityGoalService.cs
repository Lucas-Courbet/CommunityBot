using CommunityBot.Core.Goals;

namespace CommunityBot.Application.Goals;

/// <summary>
/// Provides application operations for community goals.
/// </summary>
public interface ICommunityGoalService
{
    /// <summary>
    /// Creates a goal and arms its durable activity subscription.
    /// </summary>
    Task<CommunityGoal> CreateAsync(CommunityGoalCreateRequest request, CancellationToken ct = default);
}