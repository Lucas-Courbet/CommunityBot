namespace CommunityBot.Application.Members;

/// <summary>
/// Describes the outcome of synchronizing a Discord member
/// with the application's persisted state.
/// </summary>
public enum MemberSynchronizationStatus
{
    Created,
    Reactivated,
    Updated,
    Unchanged
}