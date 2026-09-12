using CommunityBot.Application.Rewards;
using CommunityBot.Core.Rewards;
using CommunityBot.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CommunityBot.Infrastructure.Persistence.Rewards;

public sealed class RewardEntitlementRepository(AppDbContext context)
    : ABaseRepository<RewardEntitlement, long>(context), IRewardEntitlementRepository
{
    public Task<RewardEntitlement?> GetByIdForUpdateAsync(
        long entitlementId,
        CancellationToken ct = default)
        => DbSet
            .FromSqlInterpolated(
                $"""
                 SELECT *
                 FROM reward_entitlements
                 WHERE id = {entitlementId}
                 FOR UPDATE
                 """)
            .SingleOrDefaultAsync(ct);

    public Task<RewardType?> GetRewardTypeByIdAsync(
        long entitlementId,
        CancellationToken ct = default)
        => DbSet
            .AsNoTracking()
            .Where(entitlement => entitlement.Id == entitlementId)
            .Select(entitlement => (RewardType?)entitlement.RewardType)
            .SingleOrDefaultAsync(ct);
}