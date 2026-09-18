using CommunityBot.Application.Common.Persistence;
using CommunityBot.Core.Rewards;

namespace CommunityBot.Application.Rewards;

public interface IRewardEntitlementRepository
    : IBaseRepository<RewardEntitlement, long>
{
    Task<RewardEntitlement?> GetByIdForUpdateAsync(long entitlementId, CancellationToken ct = default);

    Task<RewardType?> GetRewardTypeByIdAsync(long entitlementId, CancellationToken ct = default);

    Task<RewardDeliverySnapshot?> GetDeliverySnapshotAsync(long entitlementId, CancellationToken ct = default);
}