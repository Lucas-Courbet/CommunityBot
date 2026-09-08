using CommunityBot.Application.Common.Persistence;
using CommunityBot.Core.Items;

namespace CommunityBot.Application.Items;

/// <summary>
/// Provides persistence operations for member inventory entries.
/// </summary>
public interface IInventoryRepository : IBaseRepository<InventoryItem, long>
{
    Task<InventoryItem?> GetActiveItemAsync(
        ulong memberId,
        string itemId,
        CancellationToken ct = default);

    Task<(IReadOnlyList<InventoryItem> Inventory, int Count)> GetInventoryAsync(
        InventoryRequest request,
        CancellationToken ct = default);
}