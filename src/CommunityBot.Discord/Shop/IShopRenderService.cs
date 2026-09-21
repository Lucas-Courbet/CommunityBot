using CommunityBot.Application.Shop;
using CommunityBot.Discord.Interactions;
using NetCord.Rest;

namespace CommunityBot.Discord.Shop;

/// <summary>
/// Renders Discord views for the Shop workflow.
/// </summary>
public interface IShopRenderService
{
    EmbedProperties GetHomeEmbed();

    StringMenuProperties GetCategoryMenu(string customId);

    ShopPageView RenderCategoryPage(ShopPageContext context);

    ShopPageView RenderConfirmationView(ShopItemDto item);

    ResponseRequest RenderPurchaseResult(PurchaseResult result);
}