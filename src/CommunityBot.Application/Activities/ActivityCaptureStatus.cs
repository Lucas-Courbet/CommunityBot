namespace CommunityBot.Application.Activities;

public enum ActivityCaptureStatus
{
    NotCaptured,
    Captured,
    CapturedConservatively,
    NotCapturedDueToFailure
}