namespace CommunityBot.Application.Roles;

public enum RoleOperationStatus
{
    Assigned,
    AlreadyAssigned,
    NotConfigured,
    Forbidden,
    NotFound,
    Failure
}