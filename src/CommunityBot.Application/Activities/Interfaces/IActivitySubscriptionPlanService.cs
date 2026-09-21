using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities.Interfaces;

/// <summary>
/// Coordinates durable activity subscription plans owned by consumer contexts.
/// </summary>
/// <remarks>
/// Plan mutations execute inside a caller-owned database transaction. The caller must persist its own
/// pending changes before invoking this service. The service may persist its subscription changes but
/// never commits or rolls back the caller-owned transaction.
/// </remarks>
public interface IActivitySubscriptionPlanService
{
    /// <summary>
    /// Arms the initial subscriptions for one consumer context.
    /// </summary>
    Task ArmAsync(
        ActivityConsumerType consumerType,
        string contextReference,
        IReadOnlyCollection<ActivityEventType> eventTypes,
        DateTime captureFrom,
        DateTime captureUntil,
        CancellationToken ct = default);

    /// <summary>
    /// Stops future capture for all subscriptions belonging to one consumer context.
    /// </summary>
    Task CloseAsync(
        ActivityConsumerType consumerType,
        string contextReference,
        DateTime closedAt,
        CancellationToken ct = default);

    /// <summary>
    /// Extends the capture deadline for all subscriptions belonging to one consumer context.
    /// </summary>
    Task ExtendAsync(
        ActivityConsumerType consumerType,
        string contextReference,
        DateTime captureUntil,
        CancellationToken ct = default);
}