namespace CommunityBot.Discord.Pagination;

/// <summary>
/// Handles a family of Discord pagination sources.
/// </summary>
public interface IPaginationStrategy
{
    /// <summary>
    /// Indicates whether this strategy can handle the provided source.
    /// </summary>
    bool CanHandle(string source);

    /// <summary>
    /// Builds the page matching the provided pagination request.
    /// </summary>
    Task<PaginationPage> GetPageAsync(PaginationRequest request, CancellationToken ct = default);
}