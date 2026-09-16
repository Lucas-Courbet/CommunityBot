using CommunityBot.Core.Common;
using CommunityBot.Core.Members;

namespace CommunityBot.Core.Activities;

/// <summary>
/// Immutable community activity fact captured for durable asynchronous processing.
/// </summary>
public sealed class ActivityEvent : IEntity<long>
{
    public long Id { get; init; }

    public required Guid EventId { get; init; }

    public required ActivityEventType EventType { get; init; }

    public required ulong MemberId { get; init; }

    public required DateTime OccurredAt { get; init; }

    public required int OccurrenceCount { get; init; }

    public required DateTime CapturedAt { get; init; }

    public required int ContractVersion { get; init; }

    public string? SourceReference { get; init; }

    public Member? Member { get; set; }
}