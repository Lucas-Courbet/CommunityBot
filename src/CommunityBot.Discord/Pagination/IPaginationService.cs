using NetCord.Rest;

namespace CommunityBot.Discord.Pagination;

/// <summary>
/// Creates reusable Discord navigation controls for paginated views.
/// </summary>
public interface IPaginationService
{
    /// <summary>
    /// Creates pagination buttons whose custom IDs retain the source, owner and target page.
    /// </summary>
    IReadOnlyList<ButtonProperties> CreatePaginationButtons(
        string source,
        ulong ownerId,
        int currentPage,
        int totalPages);
}