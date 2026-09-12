using CommunityBot.Application.Shop;
using CommunityBot.Discord.Pagination;

namespace CommunityBot.Discord.Shop;

public sealed class ShopPaginationStrategy(
    IShopCatalogService shopCatalogService,
    IShopRenderService shopRenderService)
    : IPaginationStrategy
{
    private const int PageSize = 6;

    public bool CanHandle(string source)
        => ShopPaginationSource.CanHandle(source);

    public async Task<PaginationPage> GetPageAsync(
        PaginationRequest request,
        CancellationToken ct = default)
    {
        if (!ShopPaginationSource.TryReadCategory(request.Source, out var category))
            throw new InvalidOperationException($"Unknown shop pagination source: {request.Source}");

        var catalog = (await shopCatalogService.GetCatalogAsync(category, ct)).ToArray();
        var totalPages = Math.Max(1, (int)Math.Ceiling(catalog.Length / (double)PageSize));
        var currentPage = Math.Clamp(request.PageIndex, 0, totalPages - 1);

        var items = catalog
            .Skip(currentPage * PageSize)
            .Take(PageSize)
            .ToArray();

        var view = shopRenderService.RenderCategoryPage(
            new ShopPageContext(
                request.Source,
                request.OwnerId,
                category,
                items,
                currentPage,
                totalPages));

        return new PaginationPage(view.Embed, view.Components);
    }
}