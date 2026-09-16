namespace CommunityBot.Infrastructure.Activities.Workers;

public sealed class ActivityConsumptionWorkerOptions
{
    public const string SectionName = "Activities:ConsumptionWorker";

    public TimeSpan IdleDelay { get; init; } = TimeSpan.FromSeconds(5);

    public TimeSpan FailureDelay { get; init; } = TimeSpan.FromSeconds(5);
}