namespace CommunityBot.Application.Roles;

/// <summary>
/// Provides access to configured roles through their stable keys.
/// </summary>
public interface IRoleConfiguration
{
    bool TryGetRoleByKey(string roleKey, out RoleDefinition role);
}