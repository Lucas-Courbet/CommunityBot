using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities.Interfaces;

/// <summary>
/// Defines a feature that consumes one category of durable activity.
/// </summary>
/// <remarks>
/// Consumers execute inside the activity consumption transaction and must not modify the
/// <see cref="ActivityConsumption"/> lifecycle themselves. External side effects must not be performed
/// from this contract. Technical failures are reported through exceptions; normal evaluation returns
/// <see cref="ActivityConsumptionOutcome.Processed"/> or <see cref="ActivityConsumptionOutcome.Ignored"/>.
/// </remarks>
public interface IActivityConsumer
{
    ActivityConsumerType ConsumerType { get; }

    Task<ActivityConsumptionOutcome> ConsumeAsync(
        ActivityEvent activityEvent,
        string contextReference,
        CancellationToken ct = default);
}