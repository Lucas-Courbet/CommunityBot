namespace CommunityBot.Application.Activities.Interfaces;

public interface IActivityConsumptionFailureService
{
    Task RecordFailureAsync(
        ActivityConsumptionProcessingException failure,
        CancellationToken ct = default);
}