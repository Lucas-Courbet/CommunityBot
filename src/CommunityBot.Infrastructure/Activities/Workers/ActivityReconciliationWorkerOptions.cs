namespace CommunityBot.Infrastructure.Activities.Workers;

public sealed class ActivityReconciliationWorkerOptions
{
    public const string SectionName = "Activities:ReconciliationWorker";

    public TimeSpan IdleDelay { get; init; } = TimeSpan.FromSeconds(5);

    public TimeSpan FailureDelay { get; init; } = TimeSpan.FromSeconds(5);
}