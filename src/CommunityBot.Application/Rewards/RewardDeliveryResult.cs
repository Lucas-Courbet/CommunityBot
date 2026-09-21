using CommunityBot.Application.Common.Results;

namespace CommunityBot.Application.Rewards;

/// <summary>
/// Represents the outcome of a reward delivery request.
/// </summary>
public sealed record RewardDeliveryResult : Result
{
    public required RewardDeliveryStatus Status { get; init; }

    public required long EntitlementId { get; init; }

    public static RewardDeliveryResult Delivered(long entitlementId)
        => new()
        {
            IsSuccess = true,
            Status = RewardDeliveryStatus.Delivered,
            EntitlementId = entitlementId
        };

    public static RewardDeliveryResult AlreadyFinalized(long entitlementId)
        => new()
        {
            IsSuccess = true,
            Status = RewardDeliveryStatus.AlreadyFinalized,
            EntitlementId = entitlementId
        };

    public static RewardDeliveryResult Failed(long entitlementId, string message)
        => new()
        {
            IsSuccess = false,
            Status = RewardDeliveryStatus.Failed,
            EntitlementId = entitlementId,
            Message = message
        };

    public static RewardDeliveryResult NotFound(long entitlementId)
        => new()
        {
            IsSuccess = false,
            Status = RewardDeliveryStatus.NotFound,
            EntitlementId = entitlementId,
            Message = "Reward entitlement not found."
        };
}