namespace CommunityBot.Infrastructure.Activities.Consumption;

public sealed class ActivityConsumptionRetryOptions
{
    public const string SectionName = "Activities:ConsumptionRetry";

    public int MaxAttempts { get; init; } = 3;

    public TimeSpan RetryDelay { get; init; } = TimeSpan.FromSeconds(5);
}