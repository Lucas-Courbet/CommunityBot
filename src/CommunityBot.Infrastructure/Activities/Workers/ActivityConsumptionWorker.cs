using CommunityBot.Application.Activities;
using CommunityBot.Application.Activities.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommunityBot.Infrastructure.Activities.Workers;

/// <summary>
/// Processes durable activity consumptions and records failed attempts through fresh dependency-injection scopes.
/// </summary>
/// <remarks>
/// Processing and failure recording intentionally use separate scopes so a failed EF Core unit of work
/// is never reused to persist retry or terminal failure state.
/// </remarks>
public sealed class ActivityConsumptionWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<ActivityConsumptionWorkerOptions> options,
    TimeProvider timeProvider,
    ILogger<ActivityConsumptionWorker> logger)
    : BackgroundService
{
    private readonly ActivityConsumptionWorkerOptions _options = options.Value;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processed = await ProcessOneAsync(stoppingToken);

                if (processed)
                    continue;

                await DelayAsync(_options.IdleDelay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (ActivityConsumptionProcessingException failure)
            {
                logger.LogError(
                    failure.InnerException ?? failure,
                    "Activity consumption {ConsumptionId} failed.",
                    failure.ConsumptionId);

                try
                {
                    await RecordFailureAsync(failure, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception recordingException)
                {
                    logger.LogError(
                        recordingException,
                        "Activity consumption failure could not be persisted for {ConsumptionId}.",
                        failure.ConsumptionId);

                    await DelayAfterFailureAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Activity consumption worker failed.");
                await DelayAfterFailureAsync(stoppingToken);
            }
        }
    }

    private async Task<bool> ProcessOneAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IActivityConsumptionService>();

        return await service.ProcessNextAsync(ct);
    }

    private async Task RecordFailureAsync(
        ActivityConsumptionProcessingException failure,
        CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IActivityConsumptionFailureService>();

        await service.RecordFailureAsync(failure, ct);
    }

    private async Task DelayAfterFailureAsync(CancellationToken ct)
    {
        try
        {
            await DelayAsync(_options.FailureDelay, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Normal host shutdown.
        }
    }

    private Task DelayAsync(TimeSpan delay, CancellationToken ct)
        => Task.Delay(delay, timeProvider, ct);
}