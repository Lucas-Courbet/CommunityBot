using CommunityBot.Core.Items;

namespace CommunityBot.Application.Shop;

/// <summary>
/// Provides read access to the currently configured shop catalog.
/// </summary>
public interface IShopCatalogService
{
    Task<ShopItemDto?> GetItemAsync(string itemId, CancellationToken ct = default);

    Task<IEnumerable<ShopItemDto>> GetCatalogAsync(
        ShopItemCategory? filterCategory = null,
        CancellationToken ct = default);
}