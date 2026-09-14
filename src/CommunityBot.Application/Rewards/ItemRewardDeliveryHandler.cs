using CommunityBot.Application.Common.Persistence;
using CommunityBot.Application.Items;
using CommunityBot.Application.Members;
using CommunityBot.Core.Rewards;
using Microsoft.Extensions.Logging;

namespace CommunityBot.Application.Rewards;

/// <summary>
/// Delivers Item reward entitlements through the canonical item inventory.
/// </summary>
public sealed class ItemRewardDeliveryHandler(
    IPersistenceContext persistenceContext,
    IRewardEntitlementRepository rewardEntitlementRepository,
    IItemRepository itemRepository,
    IMemberRepository memberRepository,
    IItemAcquisitionService itemAcquisitionService,
    ILogger<ItemRewardDeliveryHandler> logger)
    : IRewardDeliveryHandler
{
    public RewardType RewardType => RewardType.Item;

    public async Task<RewardDeliveryResult> DeliverAsync(
        long entitlementId,
        CancellationToken ct = default)
    {
        RewardDeliveryResult result;

        await using var transaction = await persistenceContext.BeginTransactionAsync(ct);

        try
        {
            result = await ProcessDeliveryAsync(entitlementId, ct);

            await persistenceContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(CancellationToken.None);

            logger.LogError(
                ex,
                "Failed to deliver Item reward entitlement {EntitlementId}.",
                entitlementId);

            throw;
        }

        return result;
    }

    private async Task<RewardDeliveryResult> ProcessDeliveryAsync(
        long entitlementId,
        CancellationToken ct)
    {
        var entitlement = await rewardEntitlementRepository.GetByIdForUpdateAsync(
            entitlementId,
            ct);

        if (entitlement is null)
            return RewardDeliveryResult.NotFound(entitlementId);

        if (entitlement.IsFinalized)
            return RewardDeliveryResult.AlreadyFinalized(entitlementId);

        EnsureCompatibleEntitlement(entitlement);

        var item = await itemRepository.GetByIdAsync(
            entitlement.RewardReference!,
            ct);

        if (item is null)
            return RecordFailure(entitlement, "Reward item was not found.");

        if (item.GrantedRoleKey is not null)
        {
            return RecordFailure(
                entitlement,
                "Role-backed items cannot be delivered by the Item reward handler.");
        }

        if (!item.IsStackable && entitlement.Quantity != 1)
        {
            return RecordFailure(
                entitlement,
                "Non-stackable reward items must have quantity 1.");
        }

        var member = await memberRepository.GetByIdForUpdateAsync(
            entitlement.MemberId,
            ct);

        if (member is null)
            return RecordFailure(entitlement, "Reward beneficiary was not found.");

        var acquisition = await itemAcquisitionService.AcquireAsync(
            entitlement.MemberId,
            item.Id,
            entitlement.Quantity,
            ct);

        if (acquisition.Status is not (
            ItemAcquisitionStatus.Acquired or
            ItemAcquisitionStatus.AlreadyOwned))
        {
            throw new InvalidOperationException(
                $"Unsupported item acquisition status {acquisition.Status}.");
        }

        entitlement.MarkAsDelivered();

        return RewardDeliveryResult.Delivered(entitlement.Id);
    }

    private static void EnsureCompatibleEntitlement(RewardEntitlement entitlement)
    {
        if (entitlement.RewardType != RewardType.Item)
        {
            throw new InvalidOperationException(
                $"Reward entitlement {entitlement.Id} has type {entitlement.RewardType} " +
                "but was routed to the Item handler.");
        }

        if (string.IsNullOrWhiteSpace(entitlement.RewardReference))
        {
            throw new InvalidOperationException(
                $"Item reward entitlement {entitlement.Id} must define a reward reference.");
        }

        if (entitlement.Quantity <= 0)
        {
            throw new InvalidOperationException(
                $"Item reward entitlement {entitlement.Id} must have a positive quantity.");
        }
    }

    private static RewardDeliveryResult RecordFailure(
        RewardEntitlement entitlement,
        string error)
    {
        entitlement.RecordDeliveryFailure(error);

        return RewardDeliveryResult.Failed(
            entitlement.Id,
            error);
    }
}