namespace CommunityBot.Core.Activities;

/// <summary>
/// Represents the normal outcome returned by an activity consumer.
/// </summary>
public enum ActivityConsumptionOutcome
{
    /// <summary>
    /// The activity was processed successfully by the consumer.
    /// </summary>
    Processed,

    /// <summary>
    /// The activity was evaluated successfully but intentionally produced no effect.
    /// </summary>
    Ignored
}