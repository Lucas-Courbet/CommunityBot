using CommunityBot.Core.Common;
using CommunityBot.Core.Members;

namespace CommunityBot.Core.Activities;

/// <summary>
/// Immutable community activity fact captured for durable asynchronous processing.
/// Consumer processing state does not belong to this entity.
/// </summary>
public sealed class ActivityEvent : IEntity<long>
{
    public long Id { get; init; }

    /// <summary>
    /// Stable opaque identity of the captured activity fact.
    /// </summary>
    public required Guid EventId { get; init; }

    public required ActivityEventType EventType { get; init; }

    /// <summary>
    /// Discord identifier of the member associated with the activity.
    /// </summary>
    public required ulong MemberId { get; init; }

    /// <summary>
    /// Timestamp at which the source activity occurred.
    /// </summary>
    public required DateTime OccurredAt { get; init; }

    public required int OccurrenceCount { get; init; }

    /// <summary>
    /// Technical timestamp at which the activity was durably captured.
    /// </summary>
    public required DateTime CapturedAt { get; init; }

    /// <summary>
    /// Version of the persisted contract for this activity event.
    /// </summary>
    public required int ContractVersion { get; init; }

    /// <summary>
    /// Optional stable reference to the source operation when one naturally exists.
    /// </summary>
    public string? SourceReference { get; init; }

    public Member? Member { get; set; }
}