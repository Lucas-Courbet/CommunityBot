using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities.Interfaces;

public interface IActivityCaptureIncidentRepository
{
    void Add(ActivityCaptureIncident incident);
}