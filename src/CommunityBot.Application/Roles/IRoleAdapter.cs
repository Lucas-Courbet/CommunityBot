namespace CommunityBot.Application.Roles;

/// <summary>
/// Technical adapter for external role operations.
/// </summary>
public interface IRoleAdapter
{
    Task<bool> UserHasRoleAsync(
        ulong memberId,
        ulong roleId,
        CancellationToken ct = default);

    Task<RoleOperationResult?> AssignRoleAsync(
        ulong memberId,
        ulong roleId,
        CancellationToken ct = default);
}