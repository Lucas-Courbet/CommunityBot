namespace CommunityBot.Application.Rewards;

public interface IRewardDeliveryService
{
    Task<RewardDeliveryResult> DeliverAsync(long entitlementId, CancellationToken ct = default);
}