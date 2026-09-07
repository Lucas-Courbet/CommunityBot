namespace CommunityBot.Core.Members;

/// <summary>
/// Represents a snapshot of a Discord member identity supplied to the domain.
/// </summary>
public sealed record MemberIdentity(
    ulong Id,
    string Username,
    string? DisplayName,
    DateTimeOffset? JoinedAt,
    bool IsBot);