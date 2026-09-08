using CommunityBot.Core.Items;

namespace CommunityBot.Tests.Core.Items;

public sealed class InventoryItemTests
{
    [Fact]
    public void CreateActive_WithValidQuantity_ShouldCreateActiveEntry()
    {
        var inventoryItem = InventoryItem.CreateActive(123UL, "welcome-badge", 2);

        Assert.Equal(123UL, inventoryItem.MemberId);
        Assert.Equal("welcome-badge", inventoryItem.ItemId);
        Assert.Equal(2, inventoryItem.Quantity);
        Assert.Equal(ItemStatus.Active, inventoryItem.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateActive_WithNonPositiveQuantity_ShouldThrow(int quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => InventoryItem.CreateActive(123UL, "welcome-badge", quantity));
    }
}