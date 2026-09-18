using CommunityBot.Application.Rewards;
using CommunityBot.Core.Rewards;
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

    public Task<RewardDeliverySnapshot?> GetDeliverySnapshotAsync(
        long entitlementId,
        CancellationToken ct = default)
        => DbSet
            .AsNoTracking()
            .Where(entitlement => entitlement.Id == entitlementId)
            .Select(entitlement => new RewardDeliverySnapshot(
                entitlement.Id,
                entitlement.MemberId,
                entitlement.RewardType,
                entitlement.RewardReference,
                entitlement.Quantity,
                entitlement.Status))
            .SingleOrDefaultAsync(ct);
}