using CommunityBot.Application.Shop;
using CommunityBot.Core.Items;
using CommunityBot.Discord.Models;
using CommunityBot.Discord.Pagination;
using CommunityBot.Discord.Rendering;
using NetCord;
using NetCord.Rest;

namespace CommunityBot.Discord.Shop;

public sealed class ShopRenderService(IPaginationService paginationService)
    : IShopRenderService
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

    public ShopPageView RenderCategoryPage(ShopPageContext context)
    {
        var embed = BuildCategoryEmbed(context);
        var components = BuildCategoryComponents(context);

        return new ShopPageView
        {
            Embed = embed,
            Components = components
        };
    }

    public ShopPageView RenderConfirmationView(ShopItemDto item)
    {
        var description = $"""
                           Are you sure you want to purchase **{item.Label}**?

                           > **Price:** {item.Price:N0} Currency
                           > **Category:** {GetCategoryLabel(item.Category)}
                           """;

        var confirmButton = new ButtonProperties(
            $"{ShopComponentIds.ConfirmPurchase}:{item.Id}",
            "Confirm purchase",
            ButtonStyle.Success);

        var cancelButton = new ButtonProperties(
            $"{ShopComponentIds.CancelPurchase}:{item.Category}",
            "Back",
            ButtonStyle.Danger);

        return new ShopPageView
        {
            Embed = EmbedFactory.Warning(description, "Confirm purchase"),
            Components = [new ActionRowProperties([confirmButton, cancelButton])]
        };
    }

    public ResponseRequest RenderPurchaseResult(PurchaseResult result)
    {
        var backButton = new ButtonProperties(
            ShopComponentIds.ReturnHome,
            "Back to shop",
            result.IsSuccess ? ButtonStyle.Success : ButtonStyle.Danger);

        var embed = result.Status switch
        {
            PurchaseResultStatus.Success => EmbedFactory.Success(
                $"""
                 You purchased **{result.ItemLabel}** for {result.PricePaid:N0} Currency.

                 New balance: **{result.RemainingBalance:N0} Currency**
                 """,
                "Purchase confirmed"),

            PurchaseResultStatus.InsufficientFunds =>
                EmbedFactory.Error("You do not have enough Currency for this purchase.", "Purchase failed"),

            PurchaseResultStatus.ItemDisabled =>
                EmbedFactory.Error("This item is currently unavailable.", "Purchase failed"),

            PurchaseResultStatus.AlreadyOwned =>
                EmbedFactory.Error("You already own this unique item.", "Purchase failed"),

            PurchaseResultStatus.ItemNotFound =>
                EmbedFactory.Error("This item no longer exists.", "Purchase failed"),

            _ => EmbedFactory.Error(
                result.Message ?? "An internal error occurred.",
                "Purchase failed")
        };

        return new ResponseRequest
        {
            Embed = embed,
            Components = [new ActionRowProperties([backButton])]
        };
    }

    private EmbedProperties BuildCategoryEmbed(ShopPageContext context)
    {
        var title = $"Community Shop — {GetCategoryLabel(context.Category)}";

        if (context.ItemsOnPage.Count == 0)
            return EmbedFactory.Create(title, "No items are currently available in this category.");

        return new EmbedProperties
        {
            Title = title,
            Description = "Select an item below to view its purchase confirmation.",
            Color = EmbedFactory.PrimaryColor,
            Fields = context.ItemsOnPage
                .Select(item => new EmbedFieldProperties
                {
                    Name = $"{item.Label} — {item.Price:N0} Currency",
                    Value = string.IsNullOrWhiteSpace(item.Description)
                        ? "*No description.*"
                        : item.Description,
                    Inline = false
                })
                .ToArray()
        };
    }

    private IReadOnlyList<IMessageComponentProperties> BuildCategoryComponents(ShopPageContext context)
    {
        var components = new List<IMessageComponentProperties>();

        if (context.ItemsOnPage.Count > 0)
            components.Add(BuildItemSelectMenu(context.ItemsOnPage));

        var buttons = paginationService.CreatePaginationButtons(
            context.PaginationSource,
            context.PaginationOwnerId,
            context.CurrentPage,
            context.TotalPages).ToList();

        buttons.Add(new ButtonProperties(
            ShopComponentIds.ReturnHome,
            "Shop home",
            ButtonStyle.Danger));

        components.Add(new ActionRowProperties(buttons));

        return components;
    }

    private static StringMenuProperties BuildItemSelectMenu(IEnumerable<ShopItemDto> items)
    {
        var options = items.Select(item =>
            new StringMenuSelectOptionProperties(item.Label, item.Id)
            {
                Description = $"{item.Price:N0} Currency"
            });

        return new StringMenuProperties(ShopComponentIds.SelectItem, options)
        {
            Placeholder = "Select an item...",
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