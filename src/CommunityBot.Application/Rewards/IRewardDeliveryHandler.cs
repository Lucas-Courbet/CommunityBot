using CommunityBot.Core.Rewards;

namespace CommunityBot.Application.Rewards;

public interface IRewardDeliveryHandler
{
    RewardType RewardType { get; }

    Task<RewardDeliveryResult> DeliverAsync(long entitlementId, CancellationToken ct = default);
}