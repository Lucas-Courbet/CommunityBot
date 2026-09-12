using CommunityBot.Application.Shop;
using CommunityBot.Core.Items;
using CommunityBot.Discord.Interactions;
using CommunityBot.Discord.Pagination;
using CommunityBot.Discord.Shop;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace CommunityBot.Discord.Modules;

public sealed class ShopInteractionModule(
    IPaginationDispatcher paginationDispatcher,
    ILogger<ShopInteractionModule> logger,
    IResponseService responseService,
    IShopPurchaseInteractionService shopPurchaseInteractionService,
    IShopCatalogService shopCatalogService,
    IShopRenderService shopRenderService)
    : ABaseInteractionModule(logger, responseService)
{
    [ComponentInteraction(ShopComponentIds.SelectCategory)]
    public Task HandleCategorySelectionAsync()
    {
        return ExecuteInteractionAsync("shop_select_category", async () =>
        {
            var interaction = (StringMenuInteraction)Context.Interaction;
            var categoryRaw = interaction.Data.SelectedValues[0];

            if (!Enum.TryParse<ShopItemCategory>(categoryRaw, out var category))
                throw new InvalidOperationException($"Invalid shop category: {categoryRaw}");

            await DisplayCategoryPageAsync(category);
        });
    }

    [ComponentInteraction(ShopComponentIds.ReturnHome)]
    public Task HandleReturnHomeAsync()
    {
        return ExecuteInteractionAsync("shop_return_home", async () =>
        {
            await Context.Interaction.SendResponseAsync(
                InteractionCallback.ModifyMessage(properties =>
                {
                    properties.Content = string.Empty;
                    properties.Embeds = [shopRenderService.GetHomeEmbed()];
                    properties.Components =
                    [
                        shopRenderService.GetCategoryMenu(ShopComponentIds.SelectCategory)
                    ];
                }));
        });
    }

    [ComponentInteraction(ShopComponentIds.SelectItem)]
    public Task HandleItemSelectionAsync()
    {
        return ExecuteInteractionAsync("shop_select_item", async () =>
        {
            var interaction = (StringMenuInteraction)Context.Interaction;
            var itemId = interaction.Data.SelectedValues[0];
            var item = await shopCatalogService.GetItemAsync(itemId);

            if (item is null)
            {
                await DisplayImmediateErrorAsync("This item is no longer available.");
                return;
            }

            var view = shopPurchaseInteractionService.PrepareConfirmation(item);

            if (view is null)
            {
                await DisplayImmediateErrorAsync("This item is currently unavailable.");
                return;
            }

            await Context.Interaction.SendResponseAsync(
                InteractionCallback.ModifyMessage(properties =>
                {
                    properties.Content = string.Empty;
                    properties.Embeds = [view.Embed];
                    properties.Components = view.Components;
                }));
        });
    }

    [ComponentInteraction(ShopComponentIds.ConfirmPurchase)]
    public Task HandleConfirmPurchaseAsync(string itemId)
    {
        return ExecuteDeferredInteractionAsync("shop_confirm_purchase", async () =>
        {
            var item = await shopCatalogService.GetItemAsync(itemId);

            if (item is null)
            {
                await ResponseService.ModifyErrorAsync(
                    Context.Interaction,
                    "This item no longer exists.");

                return;
            }

            var response = await shopPurchaseInteractionService.ExecuteAsync(
                Context.User.Id,
                item);

            await ResponseService.ModifyAsync(Context.Interaction, response);
        });
    }

    [ComponentInteraction(ShopComponentIds.CancelPurchase)]
    public Task HandleCancelPurchaseAsync(string categoryRaw)
    {
        return ExecuteInteractionAsync("shop_cancel_purchase", async () =>
        {
            if (!Enum.TryParse<ShopItemCategory>(categoryRaw, out var category))
                throw new InvalidOperationException($"Invalid shop category: {categoryRaw}");

            await DisplayCategoryPageAsync(category);
        });
    }

    private async Task DisplayCategoryPageAsync(ShopItemCategory category)
    {
        var source = ShopPaginationSource.Build(category);

        var page = await paginationDispatcher.GetPageAsync(
            new PaginationRequest(source, Context.User.Id, 0));

        if (page is null)
            throw new InvalidOperationException($"No pagination strategy found for '{source}'.");

        await Context.Interaction.SendResponseAsync(
            InteractionCallback.ModifyMessage(properties =>
            {
                properties.Content = string.Empty;
                properties.Embeds = [page.Embed];
                properties.Components = page.Components;
            }));
    }

    private Task DisplayImmediateErrorAsync(string message)
        => Context.Interaction.SendResponseAsync(
            InteractionCallback.ModifyMessage(properties =>
            {
                properties.Content = message;
                properties.Embeds = [];
                properties.Components = [];
            }));
}