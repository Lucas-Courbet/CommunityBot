using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities;

public sealed record ActivityEventCandidate(
    ActivityEventType EventType,
    ulong MemberId,
    DateTime OccurredAt,
    int OccurrenceCount,
    string? SourceReference = null);