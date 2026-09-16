using CommunityBot.Application.Activities;
using CommunityBot.Core.Activities;

namespace CommunityBot.Infrastructure.Persistence.Activities;

public sealed class ActivityEventRepository(AppDbContext context) : IActivityEventRepository
{
    public void Add(ActivityEvent activityEvent)
    {
        ArgumentNullException.ThrowIfNull(activityEvent);
        context.ActivityEvents.Add(activityEvent);
    }
}