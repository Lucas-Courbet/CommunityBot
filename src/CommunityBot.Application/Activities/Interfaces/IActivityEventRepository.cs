using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities.Interfaces;

public interface IActivityEventRepository
{
    void Add(ActivityEvent activityEvent);
}