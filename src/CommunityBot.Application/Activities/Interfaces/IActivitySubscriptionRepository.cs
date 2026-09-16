using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities.Interfaces;

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

    Task<IReadOnlyList<ActivitySubscription>> GetMatchingAsync(
        ActivityEventType eventType,
        DateTime occurredAt,
        CancellationToken ct = default);
}