using CommunityBot.Application.Shop;
using CommunityBot.Discord.Interactions;

namespace CommunityBot.Discord.Shop;

/// <summary>
/// Coordinates Shop-specific Discord purchase interactions between the application workflow and rendering.
/// </summary>
public interface IShopPurchaseInteractionService
{
    /// <summary>
    /// Prepares the confirmation view for an item.
    /// </summary>
    /// <returns>The confirmation view, or <see langword="null"/> when the item cannot currently be purchased.</returns>
    ShopPageView? PrepareConfirmation(ShopItemDto item);

    /// <summary>
    /// Executes a confirmed purchase and renders its result as an interaction response.
    /// </summary>
    Task<ResponseRequest> ExecuteAsync(
        ulong buyerId,
        ShopItemDto item,
        CancellationToken ct = default);
}