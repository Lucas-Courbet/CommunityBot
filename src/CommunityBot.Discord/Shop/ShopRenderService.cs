using CommunityBot.Core.Items;
using CommunityBot.Discord.Rendering;
using NetCord.Rest;

namespace CommunityBot.Discord.Shop;

public sealed class ShopRenderService : IShopRenderService
{
    public EmbedProperties GetHomeEmbed()
        => EmbedFactory.Create(
            "Community Shop",
            """
            Browse the available items by selecting a category below.

            Purchases use your application Currency balance.
            """);

    public StringMenuProperties GetCategoryMenu(string customId)
    {
        var options = Enum.GetValues<ShopItemCategory>()
            .Select(category => new StringMenuSelectOptionProperties(
                GetCategoryLabel(category),
                category.ToString()));

        return new StringMenuProperties(customId, options)
        {
            Placeholder = "Select a category...",
            MaxValues = 1
        };
    }

    private static string GetCategoryLabel(ShopItemCategory category)
        => category switch
        {
            ShopItemCategory.Collectible => "Collectibles",
            ShopItemCategory.Consumable => "Consumables",
            ShopItemCategory.Role => "Roles",
            _ => category.ToString()
        };
}