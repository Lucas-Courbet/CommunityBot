using CommunityBot.Core.Common;
using CommunityBot.Core.Members;

namespace CommunityBot.Core.Items;

/// <summary>
/// Represents a member's inventory entry for a canonical item.
/// </summary>
public sealed class InventoryItem : IEntity<long>, IAuditable
{
    public long Id { get; init; }

    public required string ItemId { get; init; }

    /// <summary>
    /// Discord identifier of the owning member.
    /// </summary>
    public required ulong MemberId { get; init; }

    public ItemStatus Status { get; set; } = ItemStatus.Active;

    public int Quantity { get; set; } = 1;

    public Member? Member { get; set; }

    public Item? Item { get; set; }

    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime UpdatedAt { get; set; }

    public static InventoryItem CreateActive(ulong memberId, string itemId, int quantity = 1)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Inventory quantity must be positive.");

        return new InventoryItem
        {
            MemberId = memberId,
            ItemId = itemId,
            Status = ItemStatus.Active,
            Quantity = quantity
        };
    }
}