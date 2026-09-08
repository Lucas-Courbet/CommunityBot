using CommunityBot.Application.Common.Persistence;
using CommunityBot.Core.Items;

namespace CommunityBot.Application.Items;

/// <summary>
/// Provides persistence operations for shop offers.
/// </summary>
public interface IShopItemRepository : IBaseRepository<ShopItem, string>
{
    Task<ShopItem?> GetWithItemAsync(string itemId, CancellationToken ct = default);

    Task<List<ShopItem>> GetActiveCatalogAsync(
        ShopItemCategory? filterCategory,
        CancellationToken ct = default);
}