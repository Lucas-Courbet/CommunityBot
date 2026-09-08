namespace CommunityBot.Application.Shop;

/// <summary>
/// Orchestrates the shop purchase workflow.
/// </summary>
public interface IShopPurchaseService
{
    Task<PurchaseResult> PurchaseItemAsync(
        ulong memberId,
        string itemId,
        CancellationToken ct = default);
}