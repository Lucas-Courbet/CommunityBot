namespace CommunityBot.Application.Roles;

/// <summary>
/// Provides the technical boundary for direct external role operations.
/// </summary>
/// <remarks>
/// This adapter performs no role configuration or business validation.
/// </remarks>
public interface IRoleAdapter
{
    Task<bool> UserHasRoleAsync(
        ulong memberId,
        ulong roleId,
        CancellationToken ct = default);

    /// <summary>
    /// Attempts to assign a role and returns an error result when the external operation fails.
    /// A null result indicates success.
    /// </summary>
    Task<RoleOperationResult?> AssignRoleAsync(
        ulong memberId,
        ulong roleId,
        CancellationToken ct = default);
}