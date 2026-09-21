using CommunityBot.Application.Common.Persistence;
using CommunityBot.Core.Goals;

namespace CommunityBot.Application.Goals;

/// <summary>
/// Provides persistence operations for community goals.
/// </summary>
public interface ICommunityGoalRepository : IBaseRepository<CommunityGoal, string>
{
    /// <summary>
    /// Retrieves a goal while acquiring a row lock for the current transaction.
    /// </summary>
    Task<CommunityGoal?> GetByIdForUpdateAsync(string goalId, CancellationToken ct = default);
}