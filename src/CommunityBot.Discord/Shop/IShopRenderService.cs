using NetCord.Rest;

namespace CommunityBot.Discord.Shop;

public interface IShopRenderService
{
    EmbedProperties GetHomeEmbed();

    StringMenuProperties GetCategoryMenu(string customId);
}