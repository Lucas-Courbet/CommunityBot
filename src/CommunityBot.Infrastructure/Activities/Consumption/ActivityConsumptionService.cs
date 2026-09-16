using CommunityBot.Application.Activities;
using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Application.Common.Persistence;
using CommunityBot.Core.Activities;

namespace CommunityBot.Infrastructure.Activities.Consumption;

public sealed class ActivityConsumptionService : IActivityConsumptionService
{
    private readonly IPersistenceContext _persistenceContext;
    private readonly IActivityConsumptionRepository _consumptionRepository;
    private readonly IReadOnlyDictionary<ActivityConsumerType, IActivityConsumer> _consumers;
    private readonly TimeProvider _timeProvider;

    public ActivityConsumptionService(
        IPersistenceContext persistenceContext,
        IActivityConsumptionRepository consumptionRepository,
        IEnumerable<IActivityConsumer> consumers,
        TimeProvider timeProvider)
    {
        _persistenceContext = persistenceContext;
        _consumptionRepository = consumptionRepository;
        _timeProvider = timeProvider;

        var consumerList = consumers.ToList();

        var duplicateConsumer = consumerList
            .GroupBy(consumer => consumer.ConsumerType)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateConsumer is not null)
        {
            throw new InvalidOperationException(
                $"Multiple activity consumers are registered for consumer type '{duplicateConsumer.Key}'.");
        }

        _consumers = consumerList.ToDictionary(consumer => consumer.ConsumerType);
    }

    public async Task<bool> ProcessNextAsync(CancellationToken ct = default)
    {
        await using var transaction = await _persistenceContext.BeginTransactionAsync(ct);

        var attemptedAt = _timeProvider.GetUtcNow().UtcDateTime;
        var consumption = await _consumptionRepository.GetNextPendingForUpdateAsync(attemptedAt, ct);

        if (consumption is null)
        {
            await transaction.CommitAsync(ct);
            return false;
        }

        try
        {
            var consumer = GetConsumer(consumption);

            var outcome = await consumer.ConsumeAsync(
                consumption.ActivityEvent,
                consumption.Subscription.ContextReference,
                ct);

            ApplyOutcome(consumption, outcome, attemptedAt);

            await _persistenceContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return true;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ActivityConsumptionProcessingException(
                consumption.Id,
                attemptedAt,
                ex);
        }
    }

    private IActivityConsumer GetConsumer(ActivityConsumption consumption)
    {
        var consumerType = consumption.Subscription.ConsumerType;

        if (!_consumers.TryGetValue(consumerType, out var consumer))
        {
            throw new InvalidOperationException(
                $"No activity consumer is registered for consumer type '{consumerType}'.");
        }

        return consumer;
    }

    private static void ApplyOutcome(
        ActivityConsumption consumption,
        ActivityConsumptionOutcome outcome,
        DateTime attemptedAt)
    {
        switch (outcome)
        {
            case ActivityConsumptionOutcome.Processed:
                consumption.MarkProcessed(attemptedAt);
                break;

            case ActivityConsumptionOutcome.Ignored:
                consumption.MarkIgnored(attemptedAt);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported activity consumption outcome '{outcome}'.");
        }
    }
}