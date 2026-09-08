using CommunityBot.Application.Shop;
using CommunityBot.Core.Items;
using CommunityBot.Infrastructure.Persistence.Items;

namespace CommunityBot.Tests.Integration.Shop;

[Collection(IntegrationTestCollection.Name)]
public sealed class ShopCatalogServiceIntegrationTests(PostgreSqlIntegrationFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetCatalogAsync_ShouldReturnEnabledOffersFilteredByCategory()
    {
        Context.ShopItems.AddRange(
            CreateShopItem("badge", "Community Badge", 200, ShopItemCategory.Collectible),
            CreateShopItem("boost", "Small Boost", 100, ShopItemCategory.Consumable),
            CreateShopItem(
                "disabled-badge",
                "Disabled Badge",
                50,
                ShopItemCategory.Collectible,
                isEnabled: false));

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var service = new ShopCatalogService(new ShopItemRepository(Context));

        var catalog = (await service.GetCatalogAsync(ShopItemCategory.Collectible)).ToList();

        var item = Assert.Single(catalog);

        Assert.Equal("badge", item.Id);
        Assert.Equal("Community Badge", item.Label);
        Assert.Equal(200, item.Price);
        Assert.Equal(ShopItemCategory.Collectible, item.Category);
    }

    private static ShopItem CreateShopItem(
        string id,
        string label,
        int price,
        ShopItemCategory category,
        bool isEnabled = true)
        => new()
        {
            Id = id,
            Item = new Item
            {
                Id = id,
                Label = label,
                Description = "Integration test item",
                IsStackable = false
            },
            Price = price,
            Category = category,
            IsEnabled = isEnabled
        };
}