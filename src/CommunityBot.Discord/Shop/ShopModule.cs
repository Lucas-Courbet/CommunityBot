using CommunityBot.Discord.Interactions;
using Microsoft.Extensions.Logging;
using NetCord.Services.ApplicationCommands;

namespace CommunityBot.Discord.Shop;

/// <summary>
/// Provides the Discord entry point for the interactive shop.
/// </summary>
public sealed class ShopModule(
    ILogger<ShopModule> logger,
    IResponseService responseService,
    IShopRenderService shopRenderService)
    : ABaseSlashModule(logger, responseService)
{
    [SlashCommand("shop", "Open the community shop.")]
    public Task OpenShop()
    {
        return ExecuteCommandAsync("shop", async () =>
        {
            var embed = shopRenderService.GetHomeEmbed();
            var menu = shopRenderService.GetCategoryMenu(ShopComponentIds.SelectCategory);

            await ResponseService.RespondAsync(
                Context.Interaction,
                new ResponseRequest
                {
                    Embed = embed,
                    Components = [menu],
                    IsEphemeral = true
                });
        });
    }
}