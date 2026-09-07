namespace CommunityBot.Application.Members;

/// <summary>
/// Describes the outcome of a member deactivation request.
/// </summary>
public enum MemberDeactivationStatus
{
    Deactivated,
    AlreadyInactive,
    NotFound
}