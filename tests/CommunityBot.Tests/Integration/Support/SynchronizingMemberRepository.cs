using CommunityBot.Application.Members;
using CommunityBot.Core.Members;

namespace CommunityBot.Tests.Integration.Support;

internal sealed class SynchronizingMemberRepository(
    IMemberRepository inner,
    AsyncBarrier barrier)
    : IMemberRepository
{
    public Task<Member?> GetByIdAsync(ulong id, CancellationToken ct = default)
        => inner.GetByIdAsync(id, ct);

    public Task<Member?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => inner.GetByUsernameAsync(username, ct);

    public async Task<Member?> GetByIdForUpdateAsync(ulong memberId, CancellationToken ct = default)
    {
        await barrier.SignalAndWaitAsync(ct);
        return await inner.GetByIdForUpdateAsync(memberId, ct);
    }

    public Task<IReadOnlyList<Member>> GetAllAsync(CancellationToken ct = default)
        => inner.GetAllAsync(ct);

    public Task<bool> ExistsAsync(ulong id, CancellationToken ct = default)
        => inner.ExistsAsync(id, ct);

    public void Add(Member entity) => inner.Add(entity);

    public void AddRange(IEnumerable<Member> entities) => inner.AddRange(entities);

    public void Update(Member entity) => inner.Update(entity);

    public void UpdateRange(IEnumerable<Member> entities) => inner.UpdateRange(entities);

    public void Remove(Member entity) => inner.Remove(entity);

    public void RemoveRange(IEnumerable<Member> entities) => inner.RemoveRange(entities);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => inner.SaveChangesAsync(ct);
}