namespace CommunityBot.Application.Roles;

/// <inheritdoc />
public sealed class RoleService(
    IRoleConfiguration roleConfiguration,
    IRoleAdapter roleAdapter)
    : IRoleService
{
    /// <inheritdoc />
    public async Task<RoleOperationResult> AssignConfiguredRoleByKeyAsync(
        ulong memberId,
        string roleKey,
        CancellationToken ct = default)
    {
        if (!roleConfiguration.TryGetRoleByKey(roleKey, out var role))
            return RoleOperationResult.NotConfigured(roleKey);

        if (await roleAdapter.UserHasRoleAsync(memberId, role.Id, ct))
            return RoleOperationResult.AlreadyAssigned(role.Key, role.Id);

        var adapterError = await roleAdapter.AssignRoleAsync(
            memberId,
            role.Id,
            ct);

        return adapterError ?? RoleOperationResult.Assigned(role.Key, role.Id);
    }
}