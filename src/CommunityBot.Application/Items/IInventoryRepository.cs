using CommunityBot.Application.Common.Persistence;
using CommunityBot.Core.Items;

namespace CommunityBot.Application.Items;

/// <summary>
/// Provides persistence operations for member inventory entries.
/// </summary>
public interface IInventoryRepository : IBaseRepository<InventoryItem, long>
{
    /// <summary>
    /// Retrieves the active inventory entry for a member and item, if one exists.
    /// The returned entity is tracked for mutation by the caller.
    /// </summary>
    Task<InventoryItem?> GetActiveItemAsync(
        ulong memberId,
        string itemId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the requested inventory page and the total number of matching entries.
    /// </summary>
    Task<(IReadOnlyList<InventoryItem> Inventory, int Count)> GetInventoryAsync(
        InventoryRequest request,
        CancellationToken ct = default);
}