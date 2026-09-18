using CommunityBot.Application.Activities;
using CommunityBot.Core.Activities;
using CommunityBot.Core.Members;
using CommunityBot.Infrastructure.Activities;
using CommunityBot.Infrastructure.Activities.Consumption;
using CommunityBot.Infrastructure.Persistence.Activities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CommunityBot.Tests.Integration.Activities;

[Collection(IntegrationTestCollection.Name)]
public sealed class ActivityRecoveryIntegrationTests(
    PostgreSqlIntegrationFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task RecordFailureAsync_ShouldScheduleRetry_ForTransientFailure()
    {
        var attemptedAt = DateTime.UtcNow;
        var consumptionId = await SeedConsumptionAsync(7001UL, attemptedAt);

        var timeProvider = new FixedTimeProvider(
            new DateTimeOffset(attemptedAt));

        var service = new ActivityConsumptionFailureService(
            Context,
            new ActivityConsumptionRepository(Context),
            Options.Create(new ActivityConsumptionRetryOptions
            {
                MaxAttempts = 3,
                RetryDelay = TimeSpan.FromMinutes(1)
            }),
            timeProvider);

        await service.RecordFailureAsync(
            new ActivityConsumptionProcessingException(
                consumptionId,
                attemptedAt,
                new TimeoutException("Temporary PostgreSQL timeout.")));

        await using var assertionContext = CreateDbContext();

        var consumption = await assertionContext.ActivityConsumptions
            .SingleAsync(item => item.Id == consumptionId);

        Assert.Equal(ActivityConsumptionStatus.Pending, consumption.Status);
        Assert.Equal(1, consumption.AttemptCount);

        Assert.NotNull(consumption.LastAttemptAt);
        Assert.NotNull(consumption.NextAttemptAt);

        Assert.Equal(
            attemptedAt,
            consumption.LastAttemptAt.Value,
            TimeSpan.FromMicroseconds(1));

        Assert.Equal(
            attemptedAt.AddMinutes(1),
            consumption.NextAttemptAt.Value,
            TimeSpan.FromMicroseconds(1));

        Assert.Contains("TimeoutException", consumption.LastError);
    }

    [Fact]
    public async Task ReconcileNextAsync_ShouldCreateMissingConsumptionAndRemoveMarker()
    {
        const ulong memberId = 7002UL;

        var occurredAt = DateTime.UtcNow;
        var member = CreateMember(memberId);

        var activityEvent = new ActivityEvent
        {
            EventId = Guid.NewGuid(),
            EventType = ActivityEventType.ShopPurchaseCompleted,
            MemberId = memberId,
            OccurredAt = occurredAt,
            OccurrenceCount = 1,
            CapturedAt = DateTime.UtcNow,
            ContractVersion = 1
        };

        var subscription = new ActivitySubscription
        {
            EventType = ActivityEventType.ShopPurchaseCompleted,
            ConsumerType = ActivityConsumerType.CommunityGoal,
            ContextReference = "reconciliation-goal",
            CaptureFrom = occurredAt.AddMinutes(-1),
            CaptureUntil = occurredAt.AddMinutes(1)
        };

        var reconciliation = new ActivityReconciliation(
            activityEvent,
            DateTime.UtcNow);

        Context.Members.Add(member);
        Context.ActivityEvents.Add(activityEvent);
        Context.ActivitySubscriptions.Add(subscription);
        Context.ActivityReconciliations.Add(reconciliation);

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var service = new ActivityReconciliationService(
            Context,
            new ActivityReconciliationRepository(Context),
            new ActivityCaptureGateRepository(Context),
            new ActivitySubscriptionRepository(Context),
            new ActivityConsumptionRepository(Context));

        var reconciled = await service.ReconcileNextAsync();

        Assert.True(reconciled);

        await using var assertionContext = CreateDbContext();

        Assert.False(await assertionContext.ActivityReconciliations.AnyAsync());

        var consumption = await assertionContext.ActivityConsumptions.SingleAsync();

        Assert.Equal(activityEvent.Id, consumption.ActivityEventId);
        Assert.Equal(subscription.Id, consumption.SubscriptionId);
        Assert.Equal(ActivityConsumptionStatus.Pending, consumption.Status);
    }

    private async Task<long> SeedConsumptionAsync(
        ulong memberId,
        DateTime occurredAt)
    {
        var member = CreateMember(memberId);

        var activityEvent = new ActivityEvent
        {
            EventId = Guid.NewGuid(),
            EventType = ActivityEventType.ShopPurchaseCompleted,
            MemberId = memberId,
            OccurredAt = occurredAt,
            OccurrenceCount = 1,
            CapturedAt = occurredAt,
            ContractVersion = 1
        };

        var subscription = new ActivitySubscription
        {
            EventType = ActivityEventType.ShopPurchaseCompleted,
            ConsumerType = ActivityConsumerType.CommunityGoal,
            ContextReference = $"goal-{memberId}",
            CaptureFrom = occurredAt.AddMinutes(-1),
            CaptureUntil = occurredAt.AddMinutes(1)
        };

        var consumption = new ActivityConsumption(
            activityEvent,
            subscription);

        Context.Members.Add(member);
        Context.ActivityEvents.Add(activityEvent);
        Context.ActivitySubscriptions.Add(subscription);
        Context.ActivityConsumptions.Add(consumption);

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        return consumption.Id;
    }

    private static Member CreateMember(ulong memberId)
        => Member.Create(
            new MemberIdentity(
                memberId,
                $"User{memberId}",
                $"User {memberId}",
                null,
                false));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
            => utcNow;
    }
}