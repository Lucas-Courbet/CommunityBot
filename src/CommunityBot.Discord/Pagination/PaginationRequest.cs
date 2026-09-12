namespace CommunityBot.Discord.Pagination;

/// <summary>
/// Represents a stateless request for a Discord paginated view.
/// </summary>
public sealed record PaginationRequest(
    string Source,
    ulong OwnerId,
    int PageIndex);