using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Core.Activities;
using Microsoft.EntityFrameworkCore;

namespace CommunityBot.Infrastructure.Persistence.Activities;

/// <inheritdoc />
public sealed class ActivityReconciliationRepository(AppDbContext context)
    : IActivityReconciliationRepository
{
    public void Add(ActivityReconciliation reconciliation)
    {
        ArgumentNullException.ThrowIfNull(reconciliation);
        context.ActivityReconciliations.Add(reconciliation);
    }

    /// <inheritdoc />
    public async Task<ActivityReconciliation?> GetNextForUpdateAsync(
        CancellationToken ct = default)
    {
        var reconciliation = await context.ActivityReconciliations
            .FromSqlRaw(
                """
                SELECT *
                FROM activity_reconciliations
                ORDER BY created_at, activity_event_id
                LIMIT 1
                FOR UPDATE SKIP LOCKED
                """)
            .SingleOrDefaultAsync(ct);

        if (reconciliation is null)
            return null;

        await context.Entry(reconciliation)
            .Reference(item => item.ActivityEvent)
            .LoadAsync(ct);

        return reconciliation;
    }

    public void Remove(ActivityReconciliation reconciliation)
    {
        ArgumentNullException.ThrowIfNull(reconciliation);
        context.ActivityReconciliations.Remove(reconciliation);
    }
}