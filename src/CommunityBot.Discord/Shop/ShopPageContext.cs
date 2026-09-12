using CommunityBot.Application.Shop;
using CommunityBot.Core.Items;

namespace CommunityBot.Discord.Shop;

public sealed record ShopPageContext(
    string PaginationSource,
    ulong PaginationOwnerId,
    ShopItemCategory Category,
    IReadOnlyList<ShopItemDto> ItemsOnPage,
    int CurrentPage,
    int TotalPages);