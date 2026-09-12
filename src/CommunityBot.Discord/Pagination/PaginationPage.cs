using NetCord.Rest;

namespace CommunityBot.Discord.Pagination;

/// <summary>
/// Represents a rendered Discord pagination page.
/// </summary>
public sealed record PaginationPage(
    EmbedProperties Embed,
    IReadOnlyList<IMessageComponentProperties> Components);