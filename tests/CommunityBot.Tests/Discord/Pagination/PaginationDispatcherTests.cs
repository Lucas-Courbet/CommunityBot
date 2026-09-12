using CommunityBot.Discord.Pagination;
using NetCord.Rest;

namespace CommunityBot.Tests.Discord.Pagination;

public sealed class PaginationDispatcherTests
{
    [Fact]
    public async Task GetPageAsync_ShouldUseMatchingStrategy()
    {
        var expected = new PaginationPage(
            new EmbedProperties { Title = "Expected" },
            []);

        var dispatcher = new PaginationDispatcher(
        [
            new StubStrategy("shop_", expected)
        ]);

        var result = await dispatcher.GetPageAsync(
            new PaginationRequest("shop_Collectible", 123UL, 0));

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetPageAsync_WithMultipleMatches_ShouldThrow()
    {
        var page = new PaginationPage(new EmbedProperties(), []);

        var dispatcher = new PaginationDispatcher(
        [
            new StubStrategy("shop_", page),
            new StubStrategy("shop_", page)
        ]);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.GetPageAsync(
                new PaginationRequest("shop_Collectible", 123UL, 0)));
    }

    private sealed class StubStrategy(
        string prefix,
        PaginationPage page)
        : IPaginationStrategy
    {
        public bool CanHandle(string source)
            => source.StartsWith(prefix, StringComparison.Ordinal);

        public Task<PaginationPage> GetPageAsync(
            PaginationRequest request,
            CancellationToken ct = default)
            => Task.FromResult(page);
    }
}