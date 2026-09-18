using CommunityBot.Core.Items;
using CommunityBot.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommunityBot.Tests.Integration.Shop;

[Collection(IntegrationTestCollection.Name)]
public sealed class ShopCatalogSeederIntegrationTests(
    PostgreSqlIntegrationFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task SeedAsync_ShouldCreateSyntheticCatalog()
    {
        var seeder = CreateSeeder();

        var count = await seeder.SeedAsync();

        await using var assertionContext = CreateDbContext();

        var badge = await assertionContext.ShopItems
            .Include(shopItem => shopItem.Item)
            .SingleAsync(shopItem => shopItem.Id == "community-badge");

        var token = await assertionContext.ShopItems
            .Include(shopItem => shopItem.Item)
            .SingleAsync(shopItem => shopItem.Id == "event-token");

        Assert.Equal(2, count);

        Assert.Equal("Community Badge", badge.Item.Label);
        Assert.Equal(250, badge.Price);
        Assert.Equal(ShopItemCategory.Collectible, badge.Category);
        Assert.False(badge.Item.IsStackable);
        Assert.Null(badge.Item.GrantedRoleKey);
        Assert.True(badge.IsEnabled);

        Assert.Equal("Event Token", token.Item.Label);
        Assert.Equal(75, token.Price);
        Assert.Equal(ShopItemCategory.Consumable, token.Category);
        Assert.True(token.Item.IsStackable);
        Assert.Null(token.Item.GrantedRoleKey);
        Assert.True(token.IsEnabled);
    }

    [Fact]
    public async Task SeedAsync_ShouldResynchronizeExistingSeededData()
    {
        Context.Items.Add(new Item
        {
            Id = "community-badge",
            Label = "Outdated Badge",
            Description = "Outdated description",
            IsStackable = true,
            GrantedRoleKey = "obsolete-role"
        });

        Context.ShopItems.Add(new ShopItem
        {
            Id = "community-badge",
            Price = 9999,
            Category = ShopItemCategory.Role,
            IsEnabled = false
        });

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var seeder = CreateSeeder();

        await seeder.SeedAsync();

        await using var assertionContext = CreateDbContext();

        var seeded = await assertionContext.ShopItems
            .Include(shopItem => shopItem.Item)
            .SingleAsync(shopItem => shopItem.Id == "community-badge");

        Assert.Equal("Community Badge", seeded.Item.Label);
        Assert.Equal(
            "A permanent collectible available from the community shop.",
            seeded.Item.Description);

        Assert.False(seeded.Item.IsStackable);
        Assert.Null(seeded.Item.GrantedRoleKey);

        Assert.Equal(250, seeded.Price);
        Assert.Equal(ShopItemCategory.Collectible, seeded.Category);
        Assert.True(seeded.IsEnabled);
    }

    private ShopCatalogSeeder CreateSeeder()
        => new(
            Context,
            NullLogger<ShopCatalogSeeder>.Instance);
}