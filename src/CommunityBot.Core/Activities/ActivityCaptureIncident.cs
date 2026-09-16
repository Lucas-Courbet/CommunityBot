using CommunityBot.Core.Common;

namespace CommunityBot.Core.Activities;

/// <summary>
/// Records a known loss of an activity capture after both nominal
/// and conservative persistence failed.
/// </summary>
public sealed class ActivityCaptureIncident : IEntity<long>
{
    public long Id { get; init; }

    public required ActivityEventType EventType { get; init; }

    public required ulong MemberId { get; init; }

    public required DateTime OccurredAt { get; init; }

    public required int OccurrenceCount { get; init; }

    public string? SourceReference { get; init; }

    public required DateTime DetectedAt { get; init; }

    public required string NominalError { get; init; }

    public required string ConservativeError { get; init; }
}