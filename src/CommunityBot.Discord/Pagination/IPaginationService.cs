using NetCord.Rest;

namespace CommunityBot.Discord.Pagination;

public interface IPaginationService
{
    IReadOnlyList<ButtonProperties> CreatePaginationButtons(
        string source,
        ulong ownerId,
        int currentPage,
        int totalPages);
}