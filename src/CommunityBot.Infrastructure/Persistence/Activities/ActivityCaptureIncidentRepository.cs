using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Core.Activities;

namespace CommunityBot.Infrastructure.Persistence.Activities;

public sealed class ActivityCaptureIncidentRepository(AppDbContext context)
    : IActivityCaptureIncidentRepository
{
    public void Add(ActivityCaptureIncident incident)
    {
        ArgumentNullException.ThrowIfNull(incident);
        context.ActivityCaptureIncidents.Add(incident);
    }
}