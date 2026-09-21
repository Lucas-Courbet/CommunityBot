using CommunityBot.Core.Common;

namespace CommunityBot.Core.Items;

/// <summary>
/// Represents the canonical definition of an item that can be owned by a member,
/// independently of how the item is acquired.
/// </summary>
public sealed class Item : IEntity<string>, IAuditable
{
    /// <summary>
    /// Stable semantic identifier of the item.
    /// </summary>
    public required string Id { get; init; }

    public required string Label { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Indicates whether several units can share one active inventory entry.
    /// </summary>
    public required bool IsStackable { get; set; }

    /// <summary>
    /// Optional Discord role key associated with ownership of this item.
    /// </summary>
    public string? GrantedRoleKey { get; set; }

    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime UpdatedAt { get; set; }
}