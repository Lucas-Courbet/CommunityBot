using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities;

/// <summary>
/// Represents a source activity fact proposed for durable capture.
/// It is not itself a persisted <see cref="ActivityEvent"/>.
/// </summary>
public sealed record ActivityEventCandidate(
    ActivityEventType EventType,
    ulong MemberId,
    DateTime OccurredAt,
    int OccurrenceCount,
    string? SourceReference = null);