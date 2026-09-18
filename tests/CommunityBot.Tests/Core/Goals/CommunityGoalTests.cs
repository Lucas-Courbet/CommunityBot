using CommunityBot.Core.Goals;

namespace CommunityBot.Tests.Core.Goals;

public sealed class CommunityGoalTests
{
    [Fact]
    public void ApplyProgress_ShouldCompleteGoalWithoutExceedingTarget()
    {
        var startsAt = DateTime.UtcNow.AddHours(-1);
        var endsAt = DateTime.UtcNow.AddHours(1);

        var goal = CommunityGoal.Create(
            "shopping-drive",
            "Community Shopping Drive",
            5,
            startsAt,
            endsAt);

        Assert.True(goal.ApplyProgress(3, DateTime.UtcNow));
        Assert.True(goal.ApplyProgress(4, DateTime.UtcNow));

        Assert.Equal(5, goal.CurrentCount);
        Assert.True(goal.IsCompleted);
        Assert.NotNull(goal.CompletedAt);
    }

    [Fact]
    public void ApplyProgress_ShouldIgnoreActivityOutsideGoalWindow()
    {
        var startsAt = DateTime.UtcNow.AddHours(-2);
        var endsAt = DateTime.UtcNow.AddHours(-1);

        var goal = CommunityGoal.Create(
            "expired-goal",
            "Expired Goal",
            5,
            startsAt,
            endsAt);

        var applied = goal.ApplyProgress(1, DateTime.UtcNow);

        Assert.False(applied);
        Assert.Equal(0, goal.CurrentCount);
        Assert.False(goal.IsCompleted);
    }
}