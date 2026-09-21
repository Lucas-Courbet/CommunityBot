using CommunityBot.Application.Activities;
using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Application.Common.Persistence;
using CommunityBot.Core.Activities;
using Microsoft.Extensions.Options;

namespace CommunityBot.Infrastructure.Activities.Consumption;

/// <summary>
/// Records failed consumption attempts after the processing transaction has been abandoned.
/// </summary>
public sealed class ActivityConsumptionFailureService(
    IPersistenceContext persistenceContext,
    IActivityConsumptionRepository consumptionRepository,
    IOptions<ActivityConsumptionRetryOptions> options,
    TimeProvider timeProvider)
    : IActivityConsumptionFailureService
{
    private readonly ActivityConsumptionRetryOptions _options = options.Value;

    /// <inheritdoc />
    public async Task RecordFailureAsync(
        ActivityConsumptionProcessingException failure,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(failure);

        await using var transaction = await persistenceContext.BeginTransactionAsync(ct);

        var consumption = await consumptionRepository.GetByIdForUpdateAsync(
            failure.ConsumptionId,
            ct);

        if (consumption is null)
        {
            throw new InvalidOperationException(
                $"Activity consumption '{failure.ConsumptionId}' no longer exists.");
        }

        // The processing commit may have succeeded despite the observed exception,
        // or another worker may have completed the row before this lock was acquired.
        if (consumption.Status != ActivityConsumptionStatus.Pending)
        {
            await transaction.CommitAsync(ct);
            return;
        }

        var exception = failure.InnerException ?? failure;
        var diagnostic = ActivityConsumptionFailureClassifier.Describe(exception);
        var failedAttemptNumber = consumption.AttemptCount + 1;

        if (ActivityConsumptionFailureClassifier.IsRetryable(exception) &&
            failedAttemptNumber < _options.MaxAttempts)
        {
            consumption.ScheduleRetry(
                failure.AttemptedAt,
                diagnostic,
                timeProvider.GetUtcNow().UtcDateTime + _options.RetryDelay);
        }
        else
        {
            consumption.MarkError(failure.AttemptedAt, diagnostic);
        }

        await persistenceContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}