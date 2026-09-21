using CommunityBot.Application.Items;
using CommunityBot.Core.Items;

namespace CommunityBot.Application.Shop;

/// <inheritdoc />
public sealed class ShopCatalogService(IShopItemRepository shopItemRepository)
    : IShopCatalogService
{
    /// <inheritdoc />
    public async Task<ShopItemDto?> GetItemAsync(string itemId, CancellationToken ct = default)
    {
        var shopItem = await shopItemRepository.GetWithItemAsync(itemId, ct);

        return shopItem is null
            ? null
            : MapToDto(shopItem);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ShopItemDto>> GetCatalogAsync(
        ShopItemCategory? filterCategory = null,
        CancellationToken ct = default)
    {
        var shopItems = await shopItemRepository.GetActiveCatalogAsync(filterCategory, ct);

        return shopItems.Select(MapToDto);
    }

    private static ShopItemDto MapToDto(ShopItem shopItem)
        => new(
            shopItem.Id,
            shopItem.Item.Label,
            shopItem.Item.Description,
            shopItem.Price,
            shopItem.Category,
            shopItem.Item.GrantedRoleKey,
            shopItem.IsEnabled);
}