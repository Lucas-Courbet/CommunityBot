using CommunityBot.Core.Common;
using CommunityBot.Core.Members;

namespace CommunityBot.Core.Items;

/// <summary>
/// Represents an item owned by a member.
/// </summary>
public sealed class InventoryItem : IEntity<long>, IAuditable
{
    /// <inheritdoc />
    public long Id { get; init; }

    /// <summary>
    /// Identifier of the canonical item.
    /// </summary>
    public required string ItemId { get; init; }

    /// <summary>
    /// Discord identifier of the owning member.
    /// </summary>
    public required ulong MemberId { get; init; }

    /// <summary>
    /// Current lifecycle state of this inventory entry.
    /// </summary>
    public ItemStatus Status { get; set; } = ItemStatus.Active;

    /// <summary>
    /// Number of units represented by this entry.
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Owning member.
    /// </summary>
    public Member? Member { get; set; }

    /// <summary>
    /// Canonical item definition.
    /// </summary>
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