using CommunityBot.Application.Rewards;
using CommunityBot.Core.Rewards;

namespace CommunityBot.Tests.Application.Rewards;

public sealed class RewardDeliveryServiceTests
{
    [Fact]
    public async Task DeliverAsync_ShouldReturnNotFoundWithoutCallingHandler_WhenEntitlementDoesNotExist()
    {
        const long entitlementId = 100L;

        var handler = new SpyRewardDeliveryHandler(RewardType.Currency);
        var service = new RewardDeliveryService(
            new StubRewardEntitlementRepository(null),
            [handler]);

        var result = await service.DeliverAsync(entitlementId);

        Assert.Equal(RewardDeliveryStatus.NotFound, result.Status);
        Assert.False(result.IsSuccess);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task DeliverAsync_ShouldRouteToMatchingHandlerOnly()
    {
        const long entitlementId = 200L;

        var currencyHandler = new SpyRewardDeliveryHandler(RewardType.Currency);
        var itemHandler = new SpyRewardDeliveryHandler(RewardType.Item);

        var service = new RewardDeliveryService(
            new StubRewardEntitlementRepository(RewardType.Item),
            [currencyHandler, itemHandler]);

        var result = await service.DeliverAsync(entitlementId);

        Assert.Equal(RewardDeliveryStatus.Delivered, result.Status);
        Assert.Equal(0, currencyHandler.CallCount);
        Assert.Equal(1, itemHandler.CallCount);
        Assert.Equal(entitlementId, itemHandler.LastEntitlementId);
    }

    [Fact]
    public void Constructor_ShouldRejectDuplicateHandlers()
    {
        var repository = new StubRewardEntitlementRepository(RewardType.Currency);

        Assert.Throws<InvalidOperationException>(
            () => new RewardDeliveryService(
                repository,
                [
                    new SpyRewardDeliveryHandler(RewardType.Currency),
                    new SpyRewardDeliveryHandler(RewardType.Currency)
                ]));
    }

    private sealed class StubRewardEntitlementRepository(RewardType? rewardType)
        : IRewardEntitlementRepository
    {
        public Task<RewardType?> GetRewardTypeByIdAsync(
            long entitlementId,
            CancellationToken ct = default)
            => Task.FromResult(rewardType);

        public Task<RewardEntitlement?> GetByIdAsync(long id, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<RewardEntitlement>> GetAllAsync(CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<bool> ExistsAsync(long id, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<RewardEntitlement?> GetByIdForUpdateAsync(
            long entitlementId,
            CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<RewardDeliverySnapshot?> GetDeliverySnapshotAsync(
            long entitlementId,
            CancellationToken ct = default)
            => throw new NotSupportedException();

        public void Add(RewardEntitlement entity) => throw new NotSupportedException();

        public void AddRange(IEnumerable<RewardEntitlement> entities)
            => throw new NotSupportedException();

        public void Update(RewardEntitlement entity) => throw new NotSupportedException();

        public void UpdateRange(IEnumerable<RewardEntitlement> entities)
            => throw new NotSupportedException();

        public void Remove(RewardEntitlement entity) => throw new NotSupportedException();

        public void RemoveRange(IEnumerable<RewardEntitlement> entities)
            => throw new NotSupportedException();

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => throw new NotSupportedException();
    }

    private sealed class SpyRewardDeliveryHandler(RewardType rewardType)
        : IRewardDeliveryHandler
    {
        public RewardType RewardType { get; } = rewardType;

        public int CallCount { get; private set; }

        public long? LastEntitlementId { get; private set; }

        public Task<RewardDeliveryResult> DeliverAsync(
            long entitlementId,
            CancellationToken ct = default)
        {
            CallCount++;
            LastEntitlementId = entitlementId;

            return Task.FromResult(
                RewardDeliveryResult.Delivered(entitlementId));
        }
    }
}