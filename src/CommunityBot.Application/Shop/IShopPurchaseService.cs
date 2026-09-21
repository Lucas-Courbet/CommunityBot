namespace CommunityBot.Application.Shop;

/// <summary>
/// Orchestrates the shop purchase workflow.
/// </summary>
public interface IShopPurchaseService
{
    /// <summary>
    /// Attempts to purchase a shop offer for a member.
    /// </summary>
    Task<PurchaseResult> PurchaseItemAsync(
        ulong memberId,
        string itemId,
        CancellationToken ct = default);
}