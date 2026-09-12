using CommunityBot.Core.Items;
using CommunityBot.Discord.Pagination;

namespace CommunityBot.Discord.Shop;

public static class ShopPaginationSource
{
    private const string Key = "shop";

    public static string Build(ShopItemCategory category)
        => PaginationSource.Build(Key, category);

    public static bool CanHandle(string source)
        => TryReadCategory(source, out _);

    public static bool TryReadCategory(string source, out ShopItemCategory category)
        => PaginationSource.TryReadEnum(source, Key, out category);
}