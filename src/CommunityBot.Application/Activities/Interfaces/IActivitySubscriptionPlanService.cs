using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities.Interfaces;

public interface IActivitySubscriptionPlanService
{
    Task ArmAsync(
        ActivityConsumerType consumerType,
        string contextReference,
        IReadOnlyCollection<ActivityEventType> eventTypes,
        DateTime captureFrom,
        DateTime captureUntil,
        CancellationToken ct = default);

    Task CloseAsync(
        ActivityConsumerType consumerType,
        string contextReference,
        DateTime closedAt,
        CancellationToken ct = default);

    Task ExtendAsync(
        ActivityConsumerType consumerType,
        string contextReference,
        DateTime captureUntil,
        CancellationToken ct = default);
}