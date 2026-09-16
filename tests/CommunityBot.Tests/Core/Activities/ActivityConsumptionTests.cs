using CommunityBot.Core.Activities;

namespace CommunityBot.Tests.Core.Activities;

public sealed class ActivityConsumptionTests
{
    [Fact]
    public void ScheduleRetry_ShouldKeepConsumptionPendingAndRecordAttempt()
    {
        var attemptedAt = DateTime.UtcNow;
        var nextAttemptAt = attemptedAt.AddMinutes(1);
        var consumption = CreateConsumption();

        consumption.ScheduleRetry(attemptedAt, "Temporary failure.", nextAttemptAt);

        Assert.Equal(ActivityConsumptionStatus.Pending, consumption.Status);
        Assert.Equal(1, consumption.AttemptCount);
        Assert.Equal(attemptedAt, consumption.LastAttemptAt);
        Assert.Equal(nextAttemptAt, consumption.NextAttemptAt);
        Assert.Equal("Temporary failure.", consumption.LastError);
    }

    [Fact]
    public void MarkProcessed_ShouldFinalizePendingConsumption()
    {
        var attemptedAt = DateTime.UtcNow;
        var consumption = CreateConsumption();

        consumption.MarkProcessed(attemptedAt);

        Assert.Equal(ActivityConsumptionStatus.Processed, consumption.Status);
        Assert.Equal(1, consumption.AttemptCount);
        Assert.Equal(attemptedAt, consumption.LastAttemptAt);
        Assert.Null(consumption.NextAttemptAt);

        Assert.Throws<InvalidOperationException>(
            () => consumption.MarkProcessed(attemptedAt.AddSeconds(1)));
    }

    private static ActivityConsumption CreateConsumption()
    {
        var activityEvent = new ActivityEvent
        {
            EventId = Guid.NewGuid(),
            EventType = ActivityEventType.ShopPurchaseCompleted,
            MemberId = 100UL,
            OccurredAt = DateTime.UtcNow,
            OccurrenceCount = 1,
            CapturedAt = DateTime.UtcNow,
            ContractVersion = 1
        };

        var subscription = new ActivitySubscription
        {
            EventType = ActivityEventType.ShopPurchaseCompleted,
            ConsumerType = ActivityConsumerType.CommunityGoal,
            ContextReference = "test-goal",
            CaptureFrom = DateTime.UtcNow.AddMinutes(-1),
            CaptureUntil = DateTime.UtcNow.AddMinutes(1)
        };

        return new ActivityConsumption(activityEvent, subscription);
    }
}