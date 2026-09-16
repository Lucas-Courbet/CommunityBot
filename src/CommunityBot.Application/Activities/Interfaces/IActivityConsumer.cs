using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities.Interfaces;

/// <summary>
/// Handles one category of durable community activity.
/// Implementations execute inside the activity consumption database transaction.
/// </summary>
public interface IActivityConsumer
{
    ActivityConsumerType ConsumerType { get; }

    Task<ActivityConsumptionOutcome> ConsumeAsync(
        ActivityEvent activityEvent,
        string contextReference,
        CancellationToken ct = default);
}