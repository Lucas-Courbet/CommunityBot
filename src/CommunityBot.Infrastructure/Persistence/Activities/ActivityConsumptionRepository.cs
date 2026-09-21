using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Core.Activities;
using Microsoft.EntityFrameworkCore;

namespace CommunityBot.Infrastructure.Persistence.Activities;

/// <inheritdoc />
public sealed class ActivityConsumptionRepository(AppDbContext context)
    : IActivityConsumptionRepository
{
    public void AddRange(IEnumerable<ActivityConsumption> consumptions)
    {
        ArgumentNullException.ThrowIfNull(consumptions);
        context.ActivityConsumptions.AddRange(consumptions);
    }

    /// <inheritdoc />
    public async Task<ActivityConsumption?> GetNextPendingForUpdateAsync(
        DateTime now,
        CancellationToken ct = default)
    {
        if (now.Kind != DateTimeKind.Utc)
            throw new ArgumentException(
                "Consumption eligibility time must use UTC.",
                nameof(now));

        var consumption = await context.ActivityConsumptions
            .FromSqlInterpolated(
                $"""
                 SELECT *
                 FROM activity_consumptions
                 WHERE status = 'Pending'
                   AND (next_attempt_at IS NULL OR next_attempt_at <= {now})
                 ORDER BY created_at, id
                 LIMIT 1
                 FOR UPDATE SKIP LOCKED
                 """)
            .SingleOrDefaultAsync(ct);

        if (consumption is null)
            return null;

        await context.Entry(consumption)
            .Reference(item => item.ActivityEvent)
            .LoadAsync(ct);

        await context.Entry(consumption)
            .Reference(item => item.Subscription)
            .LoadAsync(ct);

        return consumption;
    }

    /// <inheritdoc />
    public Task<ActivityConsumption?> GetByIdForUpdateAsync(
        long id,
        CancellationToken ct = default)
        => context.ActivityConsumptions
            .FromSqlInterpolated(
                $"""
                 SELECT *
                 FROM activity_consumptions
                 WHERE id = {id}
                 FOR UPDATE
                 """)
            .SingleOrDefaultAsync(ct);
}