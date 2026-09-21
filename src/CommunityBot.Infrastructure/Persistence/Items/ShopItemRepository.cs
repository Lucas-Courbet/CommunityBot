using CommunityBot.Application.Items;
using CommunityBot.Core.Items;
using Microsoft.EntityFrameworkCore;

namespace CommunityBot.Infrastructure.Persistence.Items;

/// <inheritdoc />
public sealed class ShopItemRepository(AppDbContext context)
    : ABaseRepository<ShopItem, string>(context), IShopItemRepository
{
    public async Task<ShopItem?> GetWithItemAsync(string itemId, CancellationToken ct = default)
        => await DbSet
            .AsNoTracking()
            .Include(shopItem => shopItem.Item)
            .SingleOrDefaultAsync(shopItem => shopItem.Id == itemId, ct);

    public async Task<List<ShopItem>> GetActiveCatalogAsync(
        ShopItemCategory? filterCategory,
        CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(shopItem => shopItem.Item)
            .Where(shopItem => shopItem.IsEnabled);

        if (filterCategory.HasValue)
            query = query.Where(shopItem => shopItem.Category == filterCategory.Value);

        return await query
            .OrderBy(shopItem => shopItem.Price)
            .ToListAsync(ct);
    }
}