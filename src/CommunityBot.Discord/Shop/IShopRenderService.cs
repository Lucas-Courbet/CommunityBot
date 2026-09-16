using CommunityBot.Application.Shop;
using CommunityBot.Discord.Interactions;
using NetCord.Rest;

namespace CommunityBot.Discord.Shop;

public interface IShopRenderService
{
    EmbedProperties GetHomeEmbed();

    StringMenuProperties GetCategoryMenu(string customId);

    ShopPageView RenderCategoryPage(ShopPageContext context);

    ShopPageView RenderConfirmationView(ShopItemDto item);

    ResponseRequest RenderPurchaseResult(PurchaseResult result);
}