using CommunityBot.Application.Items;
using CommunityBot.Core.Items;
using CommunityBot.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CommunityBot.Infrastructure.Persistence.Items;

public sealed class InventoryRepository(AppDbContext context)
    : ABaseRepository<InventoryItem, long>(context), IInventoryRepository
{
    public Task<InventoryItem?> GetActiveItemAsync(
        ulong memberId,
        string itemId,
        CancellationToken ct = default)
        => DbSet.SingleOrDefaultAsync(
            inventoryItem =>
                inventoryItem.MemberId == memberId &&
                inventoryItem.ItemId == itemId &&
                inventoryItem.Status == ItemStatus.Active,
            ct);

    public async Task<(IReadOnlyList<InventoryItem> Inventory, int Count)> GetInventoryAsync(
        InventoryRequest request,
        CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(inventoryItem => inventoryItem.MemberId == request.MemberId);

        var count = await query.CountAsync(ct);

        var inventory = await query
            .Include(inventoryItem => inventoryItem.Item)
            .OrderByDescending(inventoryItem => inventoryItem.CreatedAt)
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return (inventory, count);
    }
}