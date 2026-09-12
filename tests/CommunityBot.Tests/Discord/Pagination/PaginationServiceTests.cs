using CommunityBot.Discord.Pagination;

namespace CommunityBot.Tests.Discord.Pagination;

public sealed class PaginationServiceTests
{
    private readonly PaginationService _service = new();

    [Fact]
    public void CreatePaginationButtons_WithSinglePage_ShouldReturnNoButtons()
    {
        var buttons = _service.CreatePaginationButtons("shop_Collectible", 123UL, 0, 1);

        Assert.Empty(buttons);
    }

    [Fact]
    public void CreatePaginationButtons_ShouldEncodeSourceOwnerAndPages()
    {
        var buttons = _service.CreatePaginationButtons("shop_Collectible", 123UL, 1, 3);

        Assert.Equal(3, buttons.Count);
        Assert.Equal("pagination:shop_Collectible:123:0", buttons[0].CustomId);
        Assert.Equal("pagination:shop_Collectible:123:2", buttons[2].CustomId);
    }
}