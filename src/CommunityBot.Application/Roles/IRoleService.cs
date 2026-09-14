namespace CommunityBot.Application.Roles;

public interface IRoleService
{
    Task<RoleOperationResult> AssignConfiguredRoleByKeyAsync(
        ulong memberId,
        string roleKey,
        CancellationToken ct = default);
}