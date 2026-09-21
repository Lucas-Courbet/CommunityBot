using CommunityBot.Core.Common;

namespace CommunityBot.Core.Activities;

/// <summary>
/// Permanent coordination resource associated with one capturable activity type.
/// </summary>
/// <remarks>
/// Its existence does not imply that the activity type currently has active subscriptions.
/// </remarks>
public sealed class ActivityCaptureGate : IEntity<ActivityEventType>
{
    public required ActivityEventType EventType { get; init; }

    ActivityEventType IEntity<ActivityEventType>.Id => EventType;
}