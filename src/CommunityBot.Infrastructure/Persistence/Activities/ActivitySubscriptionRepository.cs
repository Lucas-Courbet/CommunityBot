using CommunityBot.Application.Activities;
using CommunityBot.Core.Activities;
using Microsoft.EntityFrameworkCore;

namespace CommunityBot.Infrastructure.Persistence.Activities;

public sealed class ActivitySubscriptionRepository(AppDbContext context)
    : IActivitySubscriptionRepository
{
    public void AddRange(IEnumerable<ActivitySubscription> subscriptions)
        => context.ActivitySubscriptions.AddRange(subscriptions);

    public async Task<IReadOnlyList<ActivityEventType>> GetEventTypesByContextAsync(
        ActivityConsumerType consumerType,
        string contextReference,
        CancellationToken ct = default)
        => await context.ActivitySubscriptions
            .AsNoTracking()
            .Where(subscription =>
                subscription.ConsumerType == consumerType &&
                subscription.ContextReference == contextReference)
            .Select(subscription => subscription.EventType)
            .Distinct()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ActivitySubscription>> GetByContextAsync(
        ActivityConsumerType consumerType,
        string contextReference,
        CancellationToken ct = default)
        => await context.ActivitySubscriptions
            .Where(subscription =>
                subscription.ConsumerType == consumerType &&
                subscription.ContextReference == contextReference)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ActivitySubscription>> GetMatchingAsync(
        ActivityEventType eventType,
        DateTime occurredAt,
        CancellationToken ct = default)
        => await context.ActivitySubscriptions
            .Where(subscription =>
                subscription.EventType == eventType &&
                subscription.CaptureFrom <= occurredAt &&
                occurredAt < subscription.CaptureUntil)
            .OrderBy(subscription => subscription.Id)
            .ToListAsync(ct);
}