using CommunityBot.Core.Common;

namespace CommunityBot.Core.Activities;

/// <summary>
/// Records a known loss of activity capture after nominal and conservative persistence both failed.
/// </summary>
/// <remarks>
/// A capture incident is a diagnostic trace, not a reconstructed activity event.
/// It must not be treated as proof that the missing event can be recreated automatically.
/// </remarks>
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