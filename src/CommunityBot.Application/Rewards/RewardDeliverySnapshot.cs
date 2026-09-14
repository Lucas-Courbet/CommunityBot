using CommunityBot.Core.Rewards;

namespace CommunityBot.Application.Rewards;

/// <summary>
/// Immutable entitlement state used to prepare an external reward delivery.
/// </summary>
public sealed record RewardDeliverySnapshot(
    long EntitlementId,
    ulong MemberId,
    RewardType RewardType,
    string? RewardReference,
    int Quantity,
    RewardEntitlementStatus Status);