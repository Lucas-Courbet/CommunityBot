using CommunityBot.Core.Common;

namespace CommunityBot.Core.Activities;

/// <summary>
/// Permanent coordination resource for one capturable activity type.
/// </summary>
public sealed class ActivityCaptureGate : IEntity<ActivityEventType>
{
    public required ActivityEventType EventType { get; init; }

    ActivityEventType IEntity<ActivityEventType>.Id => EventType;
}