namespace CommunityBot.Application.Rewards;

/// <summary>
/// Routes reward entitlement delivery to the handler responsible for its reward type.
/// </summary>
public interface IRewardDeliveryService
{
    Task<RewardDeliveryResult> DeliverAsync(long entitlementId, CancellationToken ct = default);
}