using CommunityBot.Application.Shop;
using CommunityBot.Discord.Interactions;

namespace CommunityBot.Discord.Shop;

/// <inheritdoc />
public sealed class ShopPurchaseInteractionService(
    IShopPurchaseService shopPurchaseService,
    IShopRenderService shopRenderService)
    : IShopPurchaseInteractionService
{
    /// <inheritdoc />
    public ShopPageView? PrepareConfirmation(ShopItemDto item)
        => item.IsEnabled
            ? shopRenderService.RenderConfirmationView(item)
            : null;

    /// <inheritdoc />
    public async Task<ResponseRequest> ExecuteAsync(
        ulong buyerId,
        ShopItemDto item,
        CancellationToken ct = default)
    {
        var result = await shopPurchaseService.PurchaseItemAsync(buyerId, item.Id, ct);

        return shopRenderService.RenderPurchaseResult(result);
    }
}