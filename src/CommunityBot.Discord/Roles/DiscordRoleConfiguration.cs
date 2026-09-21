using CommunityBot.Application.Roles;

namespace CommunityBot.Discord.Roles;

/// <inheritdoc />
public sealed class DiscordRoleConfiguration(
    DiscordRoleSettings settings)
    : IRoleConfiguration
{
    private readonly Dictionary<string, ulong> _roles =
        new(settings.Roles, StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool TryGetRoleByKey(string roleKey, out RoleDefinition role)
    {
        if (_roles.TryGetValue(roleKey, out var roleId) && roleId != 0)
        {
            role = new RoleDefinition(roleKey, roleId);
            return true;
        }

        role = null!;
        return false;
    }
}