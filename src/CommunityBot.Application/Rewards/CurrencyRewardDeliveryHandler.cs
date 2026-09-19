using CommunityBot.Application.Common.Persistence;
using CommunityBot.Application.Economy;
using CommunityBot.Application.Members;
using CommunityBot.Core.Economy;
using CommunityBot.Core.Members;
using CommunityBot.Core.Rewards;
using Microsoft.Extensions.Logging;

namespace CommunityBot.Application.Rewards;

/// <summary>
/// Delivers Currency reward entitlements atomically.
/// </summary>
public sealed class CurrencyRewardDeliveryHandler(
    IPersistenceContext persistenceContext,
    IRewardEntitlementRepository rewardEntitlementRepository,
    IMemberRepository memberRepository,
    ITransactionRepository transactionRepository,
    ILogger<CurrencyRewardDeliveryHandler> logger)
    : IRewardDeliveryHandler
{
    public RewardType RewardType => RewardType.Currency;

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
                "Failed to deliver Currency reward entitlement {EntitlementId}.",
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
            entitlementId, ct);

        if (entitlement is null)
            return RewardDeliveryResult.NotFound(entitlementId);

        if (entitlement.IsFinalized)
            return RewardDeliveryResult.AlreadyFinalized(entitlementId);

        EnsureCompatibleEntitlement(entitlement);

        var member = await memberRepository.GetByIdForUpdateAsync(
            entitlement.MemberId, ct);

        if (member is null)
        {
            entitlement.RecordDeliveryFailure($"Member {entitlement.MemberId} was not found.");
            return RewardDeliveryResult.Failed(entitlement.Id, "Reward beneficiary not found.");
        }

        ApplyDelivery(entitlement, member);

        return RewardDeliveryResult.Delivered(entitlement.Id);
    }

    private void ApplyDelivery(RewardEntitlement entitlement, Member member)
    {
        member.CreditCurrency(entitlement.Quantity);

        transactionRepository.Add(
            Transaction.Create(
                member.Id,
                entitlement.Quantity,
                TransactionType.Reward,
                "Reward delivery"));

        entitlement.MarkAsDelivered();
    }

    private static void EnsureCompatibleEntitlement(RewardEntitlement entitlement)
    {
        if (entitlement.RewardType != RewardType.Currency)
        {
            throw new InvalidOperationException(
                $"Reward entitlement {entitlement.Id} has type {entitlement.RewardType} " +
                "but was routed to the Currency handler.");
        }

        if (entitlement.Quantity <= 0)
        {
            throw new InvalidOperationException(
                $"Currency reward entitlement {entitlement.Id} must have a positive quantity.");
        }

        if (entitlement.RewardReference is not null)
        {
            throw new InvalidOperationException(
                $"Currency reward entitlement {entitlement.Id} must not define a reward reference.");
        }
    }
}