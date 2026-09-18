using CommunityBot.Application.Members;
using CommunityBot.Core.Members;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommunityBot.Tests.Application.Members;

public sealed class MemberServiceTests
{
    [Fact]
    public async Task SynchronizeAsync_UnknownMember_CreatesMember()
    {
        var repository = new FakeMemberRepository();
        var service = CreateService(repository);

        var identity = CreateIdentity();

        var result = await service.SynchronizeAsync(identity);

        var member = await repository.GetByIdAsync(identity.Id);

        Assert.Equal(MemberSynchronizationStatus.Created, result);
        Assert.NotNull(member);
        Assert.True(member.IsActive);
        Assert.Equal(identity.Username, member.Username);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task SynchronizeAsync_InactiveMember_ReactivatesAndSynchronizesMember()
    {
        var member = Member.Create(CreateIdentity());
        member.Deactivate();

        var repository = new FakeMemberRepository(member);
        var service = CreateService(repository);

        var updatedIdentity = new MemberIdentity(
            member.Id,
            "updated-user",
            "Updated User",
            new DateTimeOffset(
                2026, 2, 15, 12, 0, 0,
                TimeSpan.Zero),
            true);

        var result = await service.SynchronizeAsync(updatedIdentity);

        Assert.Equal(MemberSynchronizationStatus.Reactivated, result);
        Assert.True(member.IsActive);
        Assert.Equal("updated-user", member.Username);
        Assert.Equal("Updated User", member.DisplayName);
        Assert.True(member.IsBot);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task SynchronizeAsync_ActiveMemberWithChangedIdentity_UpdatesMember()
    {
        var member = Member.Create(CreateIdentity());

        var repository = new FakeMemberRepository(member);
        var service = CreateService(repository);

        var updatedIdentity = CreateIdentity() with
        {
            Username = "updated-user"
        };

        var result = await service.SynchronizeAsync(updatedIdentity);

        Assert.Equal(MemberSynchronizationStatus.Updated, result);
        Assert.Equal("updated-user", member.Username);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task SynchronizeAsync_ActiveMemberWithIdenticalIdentity_DoesNotPersistChanges()
    {
        var identity = CreateIdentity();
        var member = Member.Create(identity);

        var repository = new FakeMemberRepository(member);
        var service = CreateService(repository);

        var result = await service.SynchronizeAsync(identity);

        Assert.Equal(MemberSynchronizationStatus.Unchanged, result);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task DeactivateAsync_ActiveMember_DeactivatesMember()
    {
        var member = Member.Create(CreateIdentity());

        var repository = new FakeMemberRepository(member);
        var service = CreateService(repository);

        var result = await service.DeactivateAsync(member.Id);

        Assert.Equal(MemberDeactivationStatus.Deactivated, result);
        Assert.False(member.IsActive);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task DeactivateAsync_InactiveMember_DoesNotPersistChanges()
    {
        var member = Member.Create(CreateIdentity());
        member.Deactivate();

        var repository = new FakeMemberRepository(member);
        var service = CreateService(repository);

        var result = await service.DeactivateAsync(member.Id);

        Assert.Equal(MemberDeactivationStatus.AlreadyInactive, result);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task DeactivateAsync_UnknownMember_ReturnsNotFoundWithoutPersisting()
    {
        var repository = new FakeMemberRepository();
        var service = CreateService(repository);

        var result = await service.DeactivateAsync(999);

        Assert.Equal(MemberDeactivationStatus.NotFound, result);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    private static MemberService CreateService(
        IMemberRepository repository)
        => new(
            repository,
            NullLogger<MemberService>.Instance);

    private static MemberIdentity CreateIdentity()
        => new(
            123,
            "lucas",
            "Lucas",
            new DateTimeOffset(
                2026, 1, 15, 18, 30, 0,
                TimeSpan.Zero),
            false);

    private sealed class FakeMemberRepository(params Member[] members)
        : IMemberRepository
    {
        private readonly Dictionary<ulong, Member> _members =
            members.ToDictionary(member => member.Id);

        public int SaveChangesCallCount { get; private set; }

        public Task<Member?> GetByIdAsync(
            ulong id,
            CancellationToken ct = default)
        {
            _members.TryGetValue(id, out var member);

            return Task.FromResult(member);
        }

        public Task<IReadOnlyList<Member>> GetAllAsync(
            CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Member>>(
                _members.Values.ToList());

        public Task<bool> ExistsAsync(
            ulong id,
            CancellationToken ct = default)
            => Task.FromResult(_members.ContainsKey(id));

        public Task<Member?> GetByUsernameAsync(
            string username,
            CancellationToken ct = default)
        {
            var member = _members.Values.FirstOrDefault(
                candidate => string.Equals(
                    candidate.Username,
                    username,
                    StringComparison.Ordinal));

            return Task.FromResult(member);
        }

        public void Add(Member member)
            => _members.Add(member.Id, member);

        public void AddRange(IEnumerable<Member> members)
        {
            foreach (var member in members)
            {
                Add(member);
            }
        }

        public void Update(Member member)
            => _members[member.Id] = member;

        public void UpdateRange(IEnumerable<Member> members)
        {
            foreach (var member in members)
            {
                Update(member);
            }
        }

        public void Remove(Member member)
            => _members.Remove(member.Id);

        public void RemoveRange(IEnumerable<Member> members)
        {
            foreach (var member in members)
            {
                Remove(member);
            }
        }

        public Task<int> SaveChangesAsync(
            CancellationToken ct = default)
        {
            SaveChangesCallCount++;

            return Task.FromResult(1);
        }

        public Task<Member?> GetByIdForUpdateAsync(
            ulong memberId,
            CancellationToken ct = default)
            => GetByIdAsync(memberId, ct);
    }
}