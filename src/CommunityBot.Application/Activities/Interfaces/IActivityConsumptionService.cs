namespace CommunityBot.Application.Activities.Interfaces;

public interface IActivityConsumptionService
{
    Task<bool> ProcessNextAsync(CancellationToken ct = default);
}