using CommunityBot.Core.Items;

namespace CommunityBot.Application.Shop;

/// <summary>
/// Provides read access to the currently configured shop catalog.
/// </summary>
public interface IShopCatalogService
{
    /// <summary>
    /// Retrieves a shop offer and its canonical item data by stable identifier.
    /// </summary>
    Task<ShopItemDto?> GetItemAsync(string itemId, CancellationToken ct = default);

    /// <summary>
    /// Lists currently purchasable shop offers, optionally filtered by category.
    /// </summary>
    Task<IEnumerable<ShopItemDto>> GetCatalogAsync(
        ShopItemCategory? filterCategory = null,
        CancellationToken ct = default);
}