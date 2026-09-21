namespace CommunityBot.Application.Activities;

/// <summary>
/// Represents the outcome of proposing an activity candidate for durable capture.
/// </summary>
public enum ActivityCaptureStatus
{
    /// <summary>
    /// No active subscription required the activity to be captured.
    /// </summary>
    NotCaptured,

    /// <summary>
    /// The activity and its consumptions were captured normally.
    /// </summary>
    Captured,

    /// <summary>
    /// The activity was captured without immediate consumptions and requires reconciliation.
    /// </summary>
    CapturedConservatively,

    /// <summary>
    /// Both nominal and conservative capture failed.
    /// </summary>
    NotCapturedDueToFailure
}