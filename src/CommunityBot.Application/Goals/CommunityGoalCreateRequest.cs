namespace CommunityBot.Application.Goals;

public sealed record CommunityGoalCreateRequest(
    string Id,
    string Title,
    int TargetCount,
    DateTime StartsAt,
    DateTime EndsAt);