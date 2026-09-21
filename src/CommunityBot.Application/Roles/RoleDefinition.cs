namespace CommunityBot.Application.Roles;

/// <summary>
/// Associates a stable configured role key with its external role identifier.
/// </summary>
public sealed record RoleDefinition(
    string Key,
    ulong Id);