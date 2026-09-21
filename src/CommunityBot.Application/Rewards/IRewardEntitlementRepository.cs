using CommunityBot.Application.Common.Persistence;
using CommunityBot.Core.Rewards;

namespace CommunityBot.Application.Rewards;

/// <summary>
/// Provides persistence operations required by reward delivery workflows.
/// </summary>
public interface IRewardEntitlementRepository
    : IBaseRepository<RewardEntitlement, long>
{
    /// <summary>
    /// Retrieves an entitlement while acquiring a row lock for the current transaction.
    /// </summary>
    Task<RewardEntitlement?> GetByIdForUpdateAsync(long entitlementId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the reward type without loading the tracked entitlement.
    /// </summary>
    Task<RewardType?> GetRewardTypeByIdAsync(long entitlementId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves an immutable, non-tracked snapshot containing the state required to prepare external delivery.
    /// </summary>
    Task<RewardDeliverySnapshot?> GetDeliverySnapshotAsync(long entitlementId, CancellationToken ct = default);
}