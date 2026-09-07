using CommunityBot.Core.Members;

namespace CommunityBot.Application.Members;

/// <summary>
/// Provides application-level operations for synchronizing,
/// querying and managing Discord members.
/// </summary>
public interface IMemberService
{
    /// <summary>
    /// Synchronizes a Discord identity with the application's persisted member state.
    /// A member may be created, reactivated, updated or left unchanged.
    /// </summary>
    Task<MemberSynchronizationStatus> SynchronizeAsync(
        MemberIdentity identity,
        CancellationToken ct = default);

    /// <summary>
    /// Marks a member as inactive while retaining its persisted data.
    /// </summary>
    Task<MemberDeactivationStatus> DeactivateAsync(
        ulong id,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a member by its Discord identifier.
    /// </summary>
    Task<Member?> GetByIdAsync(
        ulong id,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a member by its exact Discord username.
    /// </summary>
    Task<Member?> GetByUsernameAsync(
        string username,
        CancellationToken ct = default);

    /// <summary>
    /// Determines whether a member is already tracked by the application.
    /// </summary>
    Task<bool> ExistsAsync(
        ulong id,
        CancellationToken ct = default);
}