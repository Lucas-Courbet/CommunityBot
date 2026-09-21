using CommunityBot.Application.Activities;
using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Application.Common.Persistence;
using CommunityBot.Application.Items;
using CommunityBot.Core.Activities;
using CommunityBot.Core.Economy;
using CommunityBot.Core.Items;
using CommunityBot.Core.Members;
using Microsoft.Extensions.Logging;

namespace CommunityBot.Application.Shop;

/// <summary>
/// Orchestrates atomic shop purchases.
/// </summary>
/// <remarks>
/// Purchase state is flushed before durable activity capture while the transaction remains
/// caller-owned and uncommitted. Activity capture uses isolated savepoints within that transaction,
/// allowing recoverable capture failures to be handled without invalidating the purchase.
/// </remarks>
public sealed class ShopPurchaseService(
    ShopPurchaseStore store,
    IPersistenceContext persistenceContext,
    IItemAcquisitionService itemAcquisitionService,
    IActivityCaptureService activityCaptureService,
    ILogger<ShopPurchaseService> logger)
    : IShopPurchaseService
{
    /// <inheritdoc />
    public async Task<PurchaseResult> PurchaseItemAsync(
        ulong memberId,
        string itemId,
        CancellationToken ct = default)
    {
        await using var transaction = await persistenceContext.BeginTransactionAsync(ct);

        try
        {
            var processing = await ProcessPurchaseAsync(memberId, itemId, ct);

            if (processing is PurchaseProcessingResult.Rejected rejected)
                return rejected.Result;

            if (processing is not PurchaseProcessingResult.Completed completed)
                throw new InvalidOperationException("Unsupported shop purchase processing result.");

            await persistenceContext.SaveChangesAsync(ct);

            await CapturePurchaseActivityAsync(completed, ct);

            var result = BuildSuccessResult(completed);

            await transaction.CommitAsync(ct);

            logger.LogInformation(
                "Shop: Member {MemberId} bought {Item} for {Price} Currency.",
                completed.Member.Id,
                completed.ShopItem.Item.Label,
                completed.ShopItem.Price);

            return result;
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
                "Shop purchase failed for member {MemberId} and item {ItemId}.",
                memberId,
                itemId);

            return PurchaseResult.Failure(
                PurchaseResultStatus.SystemError,
                "An internal error occurred.");
        }
    }

    private async Task<PurchaseProcessingResult> ProcessPurchaseAsync(
        ulong memberId,
        string itemId,
        CancellationToken ct)
    {
        var (member, shopItem) = await store.LoadPurchaseContextAsync(memberId, itemId, ct);

        if (ValidateBasicRequirements(member, shopItem) is { } error)
            return new PurchaseProcessingResult.Rejected(error);

        var acquisition = await itemAcquisitionService.AcquireAsync(
            member!.Id,
            shopItem!.Id,
            1,
            ct);

        if (acquisition.Status == ItemAcquisitionStatus.AlreadyOwned)
        {
            return new PurchaseProcessingResult.Rejected(
                PurchaseResult.Failure(
                    PurchaseResultStatus.AlreadyOwned,
                    "This unique item is already owned."));
        }

        var purchaseTransaction = ApplyPurchase(member, shopItem);

        return new PurchaseProcessingResult.Completed(
            member,
            shopItem,
            acquisition.InventoryItem,
            purchaseTransaction);
    }

    private Transaction ApplyPurchase(Member member, ShopItem shopItem)
    {
        member.DebitCurrency(shopItem.Price);
        return store.AddPurchaseTransaction(member, shopItem);
    }

    private Task<ActivityCaptureResult> CapturePurchaseActivityAsync(
        PurchaseProcessingResult.Completed purchase,
        CancellationToken ct)
        => activityCaptureService.CaptureAsync(
            new ActivityEventCandidate(
                ActivityEventType.ShopPurchaseCompleted,
                purchase.Member.Id,
                DateTime.UtcNow,
                1,
                $"shop-purchase:{purchase.PurchaseTransaction.Id}"),
            ct);

    private static PurchaseResult? ValidateBasicRequirements(Member? member, ShopItem? shopItem)
    {
        if (shopItem is null)
        {
            return PurchaseResult.Failure(
                PurchaseResultStatus.ItemNotFound,
                "Item not found.");
        }

        if (!shopItem.IsEnabled)
        {
            return PurchaseResult.Failure(
                PurchaseResultStatus.ItemDisabled,
                "This item is currently unavailable.");
        }

        if (member is null)
        {
            return PurchaseResult.Failure(
                PurchaseResultStatus.SystemError,
                "Member profile not found.");
        }

        if (member.CurrencyBalance < shopItem.Price)
        {
            return PurchaseResult.Failure(
                PurchaseResultStatus.InsufficientFunds,
                "Insufficient funds.");
        }

        return null;
    }

    private static PurchaseResult BuildSuccessResult(PurchaseProcessingResult.Completed purchase)
        => PurchaseResult.Success(
            purchase.Member.CurrencyBalance,
            purchase.ShopItem.Item.Label,
            purchase.ShopItem.Price,
            purchase.InventoryItem.Id,
            purchase.ShopItem.Item.GrantedRoleKey);

    private abstract record PurchaseProcessingResult
    {
        public sealed record Rejected(PurchaseResult Result) : PurchaseProcessingResult;

        public sealed record Completed(
            Member Member,
            ShopItem ShopItem,
            InventoryItem InventoryItem,
            Transaction PurchaseTransaction)
            : PurchaseProcessingResult;
    }
}