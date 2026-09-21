using CommunityBot.Application.Activities.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CommunityBot.Infrastructure.Activities.Workers;

/// <summary>
/// Continuously reconciles conservatively captured activities.
/// </summary>
/// <remarks>
/// Each reconciliation attempt runs in a fresh dependency-injection scope so a failed EF Core
/// unit of work is discarded before the next attempt. Pending work remains represented durably
/// by reconciliation markers.
/// </remarks>
public sealed class ActivityReconciliationWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<ActivityReconciliationWorkerOptions> options,
    TimeProvider timeProvider,
    ILogger<ActivityReconciliationWorker> logger)
    : BackgroundService
{
    private readonly ActivityReconciliationWorkerOptions _options = options.Value;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var reconciled = await ReconcileOneAsync(stoppingToken);

                if (reconciled)
                    continue;

                await DelayAsync(_options.IdleDelay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Activity reconciliation attempt failed. " +
                    "The durable reconciliation marker remains available for retry.");

                await DelayAfterFailureAsync(stoppingToken);
            }
        }
    }

    private async Task<bool> ReconcileOneAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var service = scope.ServiceProvider
            .GetRequiredService<IActivityReconciliationService>();

        return await service.ReconcileNextAsync(ct);
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