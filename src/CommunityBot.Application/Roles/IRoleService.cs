namespace CommunityBot.Application.Roles;

/// <summary>
/// Provides high-level operations on configured external roles.
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Assigns the role identified by its stable configured key.
    /// </summary>
    Task<RoleOperationResult> AssignConfiguredRoleByKeyAsync(
        ulong memberId,
        string roleKey,
        CancellationToken ct = default);
}