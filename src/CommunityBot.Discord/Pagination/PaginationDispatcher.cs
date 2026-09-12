namespace CommunityBot.Discord.Pagination;

public sealed class PaginationDispatcher(IEnumerable<IPaginationStrategy> strategies)
    : IPaginationDispatcher
{
    public async Task<PaginationPage?> GetPageAsync(
        PaginationRequest request,
        CancellationToken ct = default)
    {
        var matches = strategies
            .Where(strategy => strategy.CanHandle(request.Source))
            .ToArray();

        return matches.Length switch
        {
            0 => null,
            1 => await matches[0].GetPageAsync(request, ct),
            _ => throw new InvalidOperationException(
                $"Multiple pagination strategies found for source '{request.Source}'.")
        };
    }
}