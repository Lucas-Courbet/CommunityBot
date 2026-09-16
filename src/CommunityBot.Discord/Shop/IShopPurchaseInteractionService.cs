using CommunityBot.Application.Shop;
using CommunityBot.Discord.Interactions;

namespace CommunityBot.Discord.Shop;

public interface IShopPurchaseInteractionService
{
    ShopPageView? PrepareConfirmation(ShopItemDto item);

    Task<ResponseRequest> ExecuteAsync(
        ulong buyerId,
        ShopItemDto item,
        CancellationToken ct = default);
}