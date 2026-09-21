using CommunityBot.Core.Rewards;

namespace CommunityBot.Application.Rewards;

/// <summary>
/// Delivers reward entitlements for one specific <see cref="RewardType"/>.
/// </summary>
/// <remarks>
/// Implementations own their complete delivery workflow, including validation, concurrency control,
/// transactional boundaries and external side effects when applicable. Already finalized entitlements
/// must be handled idempotently.
/// </remarks>
public interface IRewardDeliveryHandler
{
    RewardType RewardType { get; }

    /// <summary>
    /// Attempts to deliver the specified reward entitlement.
    /// </summary>
    Task<RewardDeliveryResult> DeliverAsync(long entitlementId, CancellationToken ct = default);
}