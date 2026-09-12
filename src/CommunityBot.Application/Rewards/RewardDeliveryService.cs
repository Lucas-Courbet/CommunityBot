using CommunityBot.Core.Rewards;

namespace CommunityBot.Application.Rewards;

/// <summary>
/// Routes reward entitlements to the delivery handler responsible for their type.
/// </summary>
public sealed class RewardDeliveryService : IRewardDeliveryService
{
    private readonly IRewardEntitlementRepository _rewardEntitlementRepository;
    private readonly IReadOnlyDictionary<RewardType, IRewardDeliveryHandler> _handlers;

    public RewardDeliveryService(
        IRewardEntitlementRepository rewardEntitlementRepository,
        IEnumerable<IRewardDeliveryHandler> handlers)
    {
        _rewardEntitlementRepository = rewardEntitlementRepository;

        var handlerList = handlers.ToList();
        var duplicateType = handlerList
            .GroupBy(handler => handler.RewardType)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateType is not null)
        {
            throw new InvalidOperationException(
                $"Multiple reward delivery handlers are registered for reward type '{duplicateType.Key}'.");
        }

        _handlers = handlerList.ToDictionary(handler => handler.RewardType);
    }

    public async Task<RewardDeliveryResult> DeliverAsync(
        long entitlementId,
        CancellationToken ct = default)
    {
        var rewardType = await _rewardEntitlementRepository.GetRewardTypeByIdAsync(
            entitlementId,
            ct);

        if (rewardType is null)
            return RewardDeliveryResult.NotFound(entitlementId);

        if (!_handlers.TryGetValue(rewardType.Value, out var handler))
        {
            throw new InvalidOperationException(
                $"No reward delivery handler is registered for reward type '{rewardType.Value}'.");
        }

        return await handler.DeliverAsync(entitlementId, ct);
    }
}