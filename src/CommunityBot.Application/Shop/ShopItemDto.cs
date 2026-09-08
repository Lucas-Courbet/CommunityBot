using CommunityBot.Core.Items;

namespace CommunityBot.Application.Shop;

/// <summary>
/// Lightweight projection of a shop offer and its canonical item data.
/// </summary>
public sealed record ShopItemDto(
    string Id,
    string Label,
    string? Description,
    int Price,
    ShopItemCategory Category,
    string? GrantedRoleKey,
    bool IsEnabled);