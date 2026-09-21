using CommunityBot.Core.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityBot.Infrastructure.Persistence.Seeders;

/// <summary>
/// Synchronizes the default Shop catalog and its canonical Items.
/// </summary>
public sealed class ShopCatalogSeeder(
    AppDbContext context,
    ILogger<ShopCatalogSeeder> logger)
{
    /// <summary>
    /// Inserts missing canonical Items and Shop offers, and synchronizes their seeded data.
    /// </summary>
    public async Task<int> SeedAsync(CancellationToken ct = default)
    {
        var seeds = GetSeeds();

        foreach (var seed in seeds)
        {
            var item = await context.Items
                .FirstOrDefaultAsync(item => item.Id == seed.Id, ct);

            if (item is null)
            {
                item = new Item
                {
                    Id = seed.Id,
                    Label = seed.Label,
                    Description = seed.Description,
                    GrantedRoleKey = seed.GrantedRoleKey,
                    IsStackable = seed.IsStackable
                };

                context.Items.Add(item);
            }
            else
            {
                item.Label = seed.Label;
                item.Description = seed.Description;
                item.GrantedRoleKey = seed.GrantedRoleKey;
                item.IsStackable = seed.IsStackable;
            }

            var shopItem = await context.ShopItems
                .FirstOrDefaultAsync(shopItem => shopItem.Id == seed.Id, ct);

            if (shopItem is null)
            {
                context.ShopItems.Add(new ShopItem
                {
                    Id = seed.Id,
                    Item = item,
                    Price = seed.Price,
                    Category = seed.Category,
                    IsEnabled = seed.IsEnabled
                });
            }
            else
            {
                shopItem.Price = seed.Price;
                shopItem.Category = seed.Category;
                shopItem.IsEnabled = seed.IsEnabled;
            }
        }

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Synthetic Shop catalog seeded. {Count} offers synchronized.",
            seeds.Count);

        return seeds.Count;
    }

    private static IReadOnlyList<ShopCatalogSeed> GetSeeds()
        =>
        [
            new(
                Id: "community-badge",
                Label: "Community Badge",
                Description: "A permanent collectible available from the community shop.",
                Price: 250,
                Category: ShopItemCategory.Collectible,
                GrantedRoleKey: null,
                IsStackable: false),

            new(
                Id: "event-token",
                Label: "Event Token",
                Description: "A generic stackable consumable used to demonstrate inventory quantities.",
                Price: 75,
                Category: ShopItemCategory.Consumable,
                GrantedRoleKey: null,
                IsStackable: true)
        ];

    private sealed record ShopCatalogSeed(
        string Id,
        string Label,
        string? Description,
        int Price,
        ShopItemCategory Category,
        string? GrantedRoleKey,
        bool IsStackable,
        bool IsEnabled = true);
}