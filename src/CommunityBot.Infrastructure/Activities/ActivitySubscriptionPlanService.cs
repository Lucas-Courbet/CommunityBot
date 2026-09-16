using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Core.Activities;
using CommunityBot.Infrastructure.Persistence;

namespace CommunityBot.Infrastructure.Activities;

public sealed class ActivitySubscriptionPlanService(
    AppDbContext context,
    IActivityCaptureGateRepository captureGateRepository,
    IActivitySubscriptionRepository subscriptionRepository)
    : IActivitySubscriptionPlanService
{
    public async Task ArmAsync(
        ActivityConsumerType consumerType,
        string contextReference,
        IReadOnlyCollection<ActivityEventType> eventTypes,
        DateTime captureFrom,
        DateTime captureUntil,
        CancellationToken ct = default)
    {
        EnsureReadyForPlanMutation();
        ValidateConsumerType(consumerType);
        ValidateContextReference(contextReference);
        ValidateCaptureWindow(captureFrom, captureUntil);

        var normalizedEventTypes = NormalizeEventTypes(eventTypes);

        await LockCaptureGatesAsync(normalizedEventTypes, ct);

        var subscriptions = normalizedEventTypes
            .Select(eventType => new ActivitySubscription
            {
                EventType = eventType,
                ConsumerType = consumerType,
                ContextReference = contextReference,
                CaptureFrom = captureFrom,
                CaptureUntil = captureUntil
            })
            .ToList();

        subscriptionRepository.AddRange(subscriptions);
        await context.SaveChangesAsync(ct);
    }

    public async Task CloseAsync(
        ActivityConsumerType consumerType,
        string contextReference,
        DateTime closedAt,
        CancellationToken ct = default)
    {
        EnsureReadyForPlanMutation();
        ValidateConsumerType(consumerType);
        ValidateContextReference(contextReference);
        ValidateUtc(closedAt, nameof(closedAt));

        var eventTypes = await GetExistingPlanEventTypesAsync(
            consumerType,
            contextReference,
            ct);

        await LockCaptureGatesAsync(eventTypes, ct);

        var subscriptions = await ReloadPlanAfterLocksAsync(
            consumerType,
            contextReference,
            eventTypes,
            ct);

        foreach (var subscription in subscriptions)
            subscription.CloseAt(closedAt);

        await context.SaveChangesAsync(ct);
    }

    public async Task ExtendAsync(
        ActivityConsumerType consumerType,
        string contextReference,
        DateTime captureUntil,
        CancellationToken ct = default)
    {
        EnsureReadyForPlanMutation();
        ValidateConsumerType(consumerType);
        ValidateContextReference(contextReference);
        ValidateUtc(captureUntil, nameof(captureUntil));

        var eventTypes = await GetExistingPlanEventTypesAsync(
            consumerType,
            contextReference,
            ct);

        await LockCaptureGatesAsync(eventTypes, ct);

        var subscriptions = await ReloadPlanAfterLocksAsync(
            consumerType,
            contextReference,
            eventTypes,
            ct);

        foreach (var subscription in subscriptions)
            subscription.ExtendUntil(captureUntil);

        await context.SaveChangesAsync(ct);
    }

    private async Task<IReadOnlyList<ActivityEventType>> GetExistingPlanEventTypesAsync(
        ActivityConsumerType consumerType,
        string contextReference,
        CancellationToken ct)
    {
        var eventTypes = await subscriptionRepository.GetEventTypesByContextAsync(
            consumerType,
            contextReference,
            ct);

        if (eventTypes.Count == 0)
        {
            throw new InvalidOperationException(
                $"No activity subscription plan exists for consumer '{consumerType}' " +
                $"and context '{contextReference}'.");
        }

        return NormalizeEventTypes(eventTypes);
    }

    private async Task<IReadOnlyList<ActivitySubscription>> ReloadPlanAfterLocksAsync(
        ActivityConsumerType consumerType,
        string contextReference,
        IReadOnlyCollection<ActivityEventType> lockedEventTypes,
        CancellationToken ct)
    {
        var subscriptions = await subscriptionRepository.GetByContextAsync(
            consumerType,
            contextReference,
            ct);

        var actualEventTypes = subscriptions
            .Select(subscription => subscription.EventType)
            .Distinct()
            .OrderBy(eventType => (int)eventType)
            .ToArray();

        if (!actualEventTypes.SequenceEqual(lockedEventTypes))
        {
            throw new InvalidOperationException(
                $"Activity subscription plan for consumer '{consumerType}' and context " +
                $"'{contextReference}' changed while its coordination gates were being acquired.");
        }

        return subscriptions;
    }

    private async Task LockCaptureGatesAsync(
        IEnumerable<ActivityEventType> eventTypes,
        CancellationToken ct)
    {
        foreach (var eventType in eventTypes)
        {
            var gate = await captureGateRepository.GetForUpdateAsync(eventType, ct);

            if (gate is null)
                throw new MissingActivityCaptureGateException(eventType);
        }
    }

    private void EnsureReadyForPlanMutation()
    {
        if (context.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "Activity subscription plan mutation requires an active caller-owned transaction.");
        }

        if (context.ChangeTracker.HasChanges())
        {
            throw new InvalidOperationException(
                "Pending consumer changes must be persisted inside the active transaction " +
                "before the activity subscription plan is mutated.");
        }
    }

    private static IReadOnlyList<ActivityEventType> NormalizeEventTypes(
        IEnumerable<ActivityEventType> eventTypes)
    {
        ArgumentNullException.ThrowIfNull(eventTypes);

        var normalized = eventTypes
            .Distinct()
            .OrderBy(eventType => (int)eventType)
            .ToArray();

        if (normalized.Length == 0)
            throw new ArgumentException("At least one activity event type is required.", nameof(eventTypes));

        foreach (var eventType in normalized)
        {
            if (!Enum.IsDefined(eventType))
                throw new ArgumentOutOfRangeException(nameof(eventTypes), eventType, "Unsupported activity event type.");
        }

        return normalized;
    }

    private static void ValidateCaptureWindow(DateTime captureFrom, DateTime captureUntil)
    {
        ValidateUtc(captureFrom, nameof(captureFrom));
        ValidateUtc(captureUntil, nameof(captureUntil));

        if (captureUntil <= captureFrom)
            throw new ArgumentOutOfRangeException(nameof(captureUntil), "Capture end must be later than capture start.");
    }

    private static void ValidateConsumerType(ActivityConsumerType consumerType)
    {
        if (!Enum.IsDefined(consumerType))
            throw new ArgumentOutOfRangeException(nameof(consumerType), consumerType, "Unsupported activity consumer type.");
    }

    private static void ValidateContextReference(string contextReference)
    {
        if (string.IsNullOrWhiteSpace(contextReference))
            throw new ArgumentException("Subscription context reference cannot be blank.", nameof(contextReference));
    }

    private static void ValidateUtc(DateTime value, string paramName)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Activity timestamps must use UTC.", paramName);
    }
}