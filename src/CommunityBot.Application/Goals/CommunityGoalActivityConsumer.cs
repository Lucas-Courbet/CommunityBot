using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Goals;

/// <summary>
/// Applies captured shop-purchase activity to the associated community goal.
/// </summary>
public sealed class CommunityGoalActivityConsumer(
    ICommunityGoalRepository goalRepository)
    : IActivityConsumer
{
    public ActivityConsumerType ConsumerType => ActivityConsumerType.CommunityGoal;

    public async Task<ActivityConsumptionOutcome> ConsumeAsync(
        ActivityEvent activityEvent,
        string contextReference,
        CancellationToken ct = default)
    {
        if (activityEvent.EventType != ActivityEventType.ShopPurchaseCompleted)
            return ActivityConsumptionOutcome.Ignored;

        var goal = await goalRepository.GetByIdForUpdateAsync(contextReference, ct);

        if (goal is null)
        {
            throw new InvalidOperationException(
                $"Community goal '{contextReference}' no longer exists.");
        }

        return goal.ApplyProgress(
            activityEvent.OccurrenceCount,
            activityEvent.OccurredAt)
            ? ActivityConsumptionOutcome.Processed
            : ActivityConsumptionOutcome.Ignored;
    }
}