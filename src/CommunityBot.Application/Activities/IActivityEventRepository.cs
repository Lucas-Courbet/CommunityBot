using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities;

public interface IActivityEventRepository
{
    void Add(ActivityEvent activityEvent);
}