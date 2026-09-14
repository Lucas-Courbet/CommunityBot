using CommunityBot.Application.Rewards;
using CommunityBot.Core.Rewards;

namespace CommunityBot.Tests.Integration.Support;

internal sealed class SynchronizingRewardEntitlementRepository(
    IRewardEntitlementRepository inner,
    AsyncBarrier barrier)
    : IRewardEntitlementRepository
{
    public Task<RewardEntitlement?> GetByIdAsync(long id, CancellationToken ct = default)
        => inner.GetByIdAsync(id, ct);

    public Task<IReadOnlyList<RewardEntitlement>> GetAllAsync(CancellationToken ct = default)
        => inner.GetAllAsync(ct);

    public Task<bool> ExistsAsync(long id, CancellationToken ct = default)
        => inner.ExistsAsync(id, ct);

    public async Task<RewardEntitlement?> GetByIdForUpdateAsync(
        long entitlementId,
        CancellationToken ct = default)
    {
        await barrier.SignalAndWaitAsync(ct);
        return await inner.GetByIdForUpdateAsync(entitlementId, ct);
    }

    public Task<RewardType?> GetRewardTypeByIdAsync(
        long entitlementId,
        CancellationToken ct = default)
        => inner.GetRewardTypeByIdAsync(entitlementId, ct);

    public Task<RewardDeliverySnapshot?> GetDeliverySnapshotAsync(
        long entitlementId,
        CancellationToken ct = default)
        => inner.GetDeliverySnapshotAsync(entitlementId, ct);

    public void Add(RewardEntitlement entity) => inner.Add(entity);

    public void AddRange(IEnumerable<RewardEntitlement> entities) => inner.AddRange(entities);

    public void Update(RewardEntitlement entity) => inner.Update(entity);

    public void UpdateRange(IEnumerable<RewardEntitlement> entities) => inner.UpdateRange(entities);

    public void Remove(RewardEntitlement entity) => inner.Remove(entity);

    public void RemoveRange(IEnumerable<RewardEntitlement> entities) => inner.RemoveRange(entities);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => inner.SaveChangesAsync(ct);
}