namespace CommunityBot.Discord.Pagination;

public interface IPaginationStrategy
{
    bool CanHandle(string source);

    Task<PaginationPage> GetPageAsync(PaginationRequest request, CancellationToken ct = default);
}