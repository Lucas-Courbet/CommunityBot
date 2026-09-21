namespace CommunityBot.Discord.Pagination;

/// <summary>
/// Routes pagination requests to the strategy responsible for their source.
/// </summary>
public interface IPaginationDispatcher
{
    /// <summary>
    /// Resolves and renders the requested page.
    /// </summary>
    /// <returns>The rendered page, or <see langword="null"/> when no strategy can handle the source.</returns>
    Task<PaginationPage?> GetPageAsync(PaginationRequest request, CancellationToken ct = default);
}