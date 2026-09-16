using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Core.Activities;
using Microsoft.EntityFrameworkCore;

namespace CommunityBot.Infrastructure.Persistence.Activities;

public sealed class ActivityCaptureGateRepository(AppDbContext context)
    : IActivityCaptureGateRepository
{
    public Task<ActivityCaptureGate?> GetForUpdateAsync(
        ActivityEventType eventType,
        CancellationToken ct = default)
    {
        var persistedEventType = eventType.ToString();

        return context.ActivityCaptureGates
            .FromSqlInterpolated(
                $"""
                 SELECT *
                 FROM activity_capture_gates
                 WHERE event_type = {persistedEventType}
                 FOR UPDATE
                 """)
            .SingleOrDefaultAsync(ct);
    }
}