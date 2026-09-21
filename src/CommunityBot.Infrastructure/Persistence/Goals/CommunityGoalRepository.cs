using CommunityBot.Application.Goals;
using CommunityBot.Core.Goals;
using Microsoft.EntityFrameworkCore;

namespace CommunityBot.Infrastructure.Persistence.Goals;

/// <inheritdoc />
public sealed class CommunityGoalRepository(AppDbContext context)
    : ABaseRepository<CommunityGoal, string>(context), ICommunityGoalRepository
{
    /// <inheritdoc />
    public Task<CommunityGoal?> GetByIdForUpdateAsync(
        string goalId,
        CancellationToken ct = default)
        => DbSet
            .FromSqlInterpolated(
                $"""
                 SELECT *
                 FROM community_goals
                 WHERE id = {goalId}
                 FOR UPDATE
                 """)
            .SingleOrDefaultAsync(ct);
}