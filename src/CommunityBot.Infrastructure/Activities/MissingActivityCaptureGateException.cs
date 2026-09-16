using CommunityBot.Core.Activities;

namespace CommunityBot.Infrastructure.Activities;

public sealed class MissingActivityCaptureGateException(ActivityEventType eventType)
    : InvalidOperationException($"No activity capture gate exists for event type '{eventType}'.");