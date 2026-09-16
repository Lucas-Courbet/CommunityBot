namespace CommunityBot.Application.Activities;

public sealed record ActivityCaptureResult(
    ActivityCaptureStatus Status,
    Guid? EventId);