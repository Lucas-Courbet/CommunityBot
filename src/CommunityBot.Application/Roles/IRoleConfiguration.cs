namespace CommunityBot.Application.Roles;

public interface IRoleConfiguration
{
    bool TryGetRoleByKey(string roleKey, out RoleDefinition role);
}