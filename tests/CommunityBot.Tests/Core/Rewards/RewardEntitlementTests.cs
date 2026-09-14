using CommunityBot.Core.Rewards;

namespace CommunityBot.Tests.Core.Rewards;

public sealed class RewardEntitlementTests
{
    [Fact]
    public void MarkAsDelivered_ShouldFinalizeAndPreservePreviousFailureHistory()
    {
        var entitlement = CreateEntitlement();

        entitlement.RecordDeliveryFailure("Temporary delivery failure.");
        entitlement.MarkAsDelivered();

        Assert.Equal(RewardEntitlementStatus.Delivered, entitlement.Status);
        Assert.Equal(2, entitlement.AttemptCount);
        Assert.Equal("Temporary delivery failure.", entitlement.LastError);
        Assert.NotNull(entitlement.DeliveredAt);
        Assert.True(entitlement.IsFinalized);
    }

    [Fact]
    public void FinalizedEntitlement_ShouldRejectFurtherLifecycleChanges()
    {
        var entitlement = CreateEntitlement();

        entitlement.MarkAsDelivered();

        Assert.Throws<InvalidOperationException>(() => entitlement.MarkAsDelivered());
        Assert.Throws<InvalidOperationException>(
            () => entitlement.RecordDeliveryFailure("Too late."));
    }

    private static RewardEntitlement CreateEntitlement()
        => new()
        {
            MemberId = 100UL,
            SourceReference = "test-reward",
            RewardType = RewardType.Currency,
            Quantity = 10
        };
}