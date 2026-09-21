using CommunityBot.Application.Shop;
using CommunityBot.Core.Items;

namespace CommunityBot.Discord.Shop;

/// <summary>
/// Provides the data required to render one page of a Shop category.
/// </summary>
public sealed record ShopPageContext(
    string PaginationSource,
    ulong PaginationOwnerId,
    ShopItemCategory Category,
    IReadOnlyList<ShopItemDto> ItemsOnPage,
    int CurrentPage,
    int TotalPages);