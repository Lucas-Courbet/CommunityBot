namespace CommunityBot.Discord.Roles;

public sealed record DiscordRoleSettings(
    ulong GuildId,
    IReadOnlyDictionary<string, ulong> Roles);