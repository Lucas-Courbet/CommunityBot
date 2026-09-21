using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities.Interfaces;

/// <summary>
/// Provides persistence queries for durable activity subscriptions.
/// </summary>
public interface IActivitySubscriptionRepository
{
    void AddRange(IEnumerable<ActivitySubscription> subscriptions);

    Task<IReadOnlyList<ActivityEventType>> GetEventTypesByContextAsync(
        ActivityConsumerType consumerType,
        string contextReference,
        CancellationToken ct = default);

    Task<IReadOnlyList<ActivitySubscription>> GetByContextAsync(
        ActivityConsumerType consumerType,
        string contextReference,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves subscriptions whose capture window contains the supplied occurrence timestamp.
    /// The beginning of the window is inclusive and the end is exclusive.
    /// </summary>
    Task<IReadOnlyList<ActivitySubscription>> GetMatchingAsync(
        ActivityEventType eventType,
        DateTime occurredAt,
        CancellationToken ct = default);
}