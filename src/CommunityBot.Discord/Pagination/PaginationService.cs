using NetCord;
using NetCord.Rest;

namespace CommunityBot.Discord.Pagination;

public sealed class PaginationService : IPaginationService
{
    public IReadOnlyList<ButtonProperties> CreatePaginationButtons(
        string source,
        ulong ownerId,
        int currentPage,
        int totalPages)
    {
        if (totalPages <= 1)
            return [];

        var previousButton = new ButtonProperties(
            $"pagination:{source}:{ownerId}:{currentPage - 1}",
            "Previous",
            ButtonStyle.Secondary)
        {
            Disabled = currentPage <= 0
        };

        var indicator = new ButtonProperties(
            $"pagination:ignore:{ownerId}:0",
            $"{currentPage + 1} / {totalPages}",
            ButtonStyle.Secondary)
        {
            Disabled = true
        };

        var nextButton = new ButtonProperties(
            $"pagination:{source}:{ownerId}:{currentPage + 1}",
            "Next",
            ButtonStyle.Secondary)
        {
            Disabled = currentPage >= totalPages - 1
        };

        return [previousButton, indicator, nextButton];
    }
}