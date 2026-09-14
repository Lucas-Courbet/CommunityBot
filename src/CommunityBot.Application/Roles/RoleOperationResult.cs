using CommunityBot.Application.Common.Results;

namespace CommunityBot.Application.Roles;

public sealed record RoleOperationResult : Result
{
    public required RoleOperationStatus Status { get; init; }

    public string? RoleKey { get; init; }

    public ulong? RoleId { get; init; }

    public static RoleOperationResult Assigned(string roleKey, ulong roleId)
        => new()
        {
            IsSuccess = true,
            Status = RoleOperationStatus.Assigned,
            RoleKey = roleKey,
            RoleId = roleId
        };

    public static RoleOperationResult AlreadyAssigned(string roleKey, ulong roleId)
        => new()
        {
            IsSuccess = false,
            Status = RoleOperationStatus.AlreadyAssigned,
            Message = "The member already has this role.",
            RoleKey = roleKey,
            RoleId = roleId
        };

    public static RoleOperationResult NotConfigured(string roleKey)
        => new()
        {
            IsSuccess = false,
            Status = RoleOperationStatus.NotConfigured,
            Message = $"Role key '{roleKey}' is not configured.",
            RoleKey = roleKey
        };

    public static RoleOperationResult Forbidden()
        => new()
        {
            IsSuccess = false,
            Status = RoleOperationStatus.Forbidden,
            Message = "The bot cannot manage this role."
        };

    public static RoleOperationResult NotFound()
        => new()
        {
            IsSuccess = false,
            Status = RoleOperationStatus.NotFound,
            Message = "Discord member or role not found."
        };

    public static RoleOperationResult Failure(string message)
        => new()
        {
            IsSuccess = false,
            Status = RoleOperationStatus.Failure,
            Message = message
        };
}