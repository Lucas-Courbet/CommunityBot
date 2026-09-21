using CommunityBot.Core.Common;

namespace CommunityBot.Core.Activities;

/// <summary>
/// Marks a conservatively captured activity event that still requires reconciliation against historical subscriptions.
/// </summary>
/// <remarks>
/// The row exists only while reconciliation is pending. Its primary key is also the foreign key of the associated
/// <see cref="ActivityEvent"/>, so at most one reconciliation can be pending for an event.
/// </remarks>
public sealed class ActivityReconciliation : IEntity<long>
{
    private ActivityReconciliation()
    {
    }

    public ActivityReconciliation(ActivityEvent activityEvent, DateTime createdAt)
    {
        ArgumentNullException.ThrowIfNull(activityEvent);

        if (createdAt.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Reconciliation timestamp must use UTC.", nameof(createdAt));

        ActivityEvent = activityEvent;
        CreatedAt = createdAt;
    }

    public long ActivityEventId { get; private set; }

    public ActivityEvent ActivityEvent { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    long IEntity<long>.Id => ActivityEventId;
}