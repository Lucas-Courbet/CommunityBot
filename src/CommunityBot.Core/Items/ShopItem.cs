using CommunityBot.Core.Common;

namespace CommunityBot.Core.Items;

/// <summary>
/// Represents the shop offer associated with a canonical item.
/// </summary>
public sealed class ShopItem : IEntity<string>, IAuditable
{
    /// <summary>
    /// Identifier shared with the canonical item.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Canonical item sold by this offer.
    /// </summary>
    public Item Item { get; set; } = null!;

    /// <summary>
    /// Price expressed in application Currency.
    /// </summary>
    public required int Price { get; set; }

    /// <summary>
    /// Category used to organize the shop catalog.
    /// </summary>
    public required ShopItemCategory Category { get; set; }

    /// <summary>
    /// Indicates whether the offer is visible and purchasable.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime UpdatedAt { get; set; }
}