namespace CommunityBot.Discord.Pagination;

public interface IPaginationDispatcher
{
    Task<PaginationPage?> GetPageAsync(
        PaginationRequest request,
        CancellationToken ct = default);
}