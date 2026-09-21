using CommunityBot.Core.Common;

namespace CommunityBot.Core.Goals;

/// <summary>
/// Represents a time-bounded community objective progressed by captured activity.
/// </summary>
public sealed class CommunityGoal : IEntity<string>, IAuditable
{
    private CommunityGoal()
    {
    }

    public string Id { get; private set; } = null!;

    public string Title { get; private set; } = null!;

    public int TargetCount { get; private set; }

    public int CurrentCount { get; private set; }

    public DateTime StartsAt { get; private set; }

    public DateTime EndsAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime UpdatedAt { get; set; }

    public bool IsCompleted => CompletedAt is not null;

    public static CommunityGoal Create(
        string id,
        string title,
        int targetCount,
        DateTime startsAt,
        DateTime endsAt)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Goal identifier cannot be blank.", nameof(id));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Goal title cannot be blank.", nameof(title));

        if (targetCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetCount), "Goal target must be strictly positive.");

        ValidateUtc(startsAt, nameof(startsAt));
        ValidateUtc(endsAt, nameof(endsAt));

        if (endsAt <= startsAt)
            throw new ArgumentOutOfRangeException(nameof(endsAt), "Goal end must be later than its start.");

        return new CommunityGoal
        {
            Id = id,
            Title = title,
            TargetCount = targetCount,
            StartsAt = startsAt,
            EndsAt = endsAt
        };
    }

    /// <summary>
    /// Applies progress occurring within the active goal window.
    /// </summary>
    /// <remarks>
    /// Progress is capped at the target. The first accepted update reaching the target
    /// records the activity occurrence timestamp as the completion time.
    /// </remarks>
    /// <returns>
    /// <see langword="true"/> when the progress was applied;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool ApplyProgress(int amount, DateTime occurredAt)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Progress amount must be strictly positive.");

        ValidateUtc(occurredAt, nameof(occurredAt));

        if (IsCompleted || !IsActiveAt(occurredAt))
            return false;

        CurrentCount = (int)Math.Min(TargetCount, (long)CurrentCount + amount);

        if (CurrentCount == TargetCount)
            CompletedAt = occurredAt;

        return true;
    }

    /// <summary>
    /// Determines whether a UTC occurrence timestamp falls within the goal window,
    /// including its start and excluding its end.
    /// </summary>
    public bool IsActiveAt(DateTime occurredAt)
    {
        ValidateUtc(occurredAt, nameof(occurredAt));
        return StartsAt <= occurredAt && occurredAt < EndsAt;
    }

    private static void ValidateUtc(DateTime value, string paramName)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Goal timestamps must use UTC.", paramName);
    }
}