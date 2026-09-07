using CommunityBot.Application.Members;
using CommunityBot.Core.Members;
using CommunityBot.Infrastructure.Persistence.Members;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommunityBot.Tests.Integration.Members;

[Collection(IntegrationTestCollection.Name)]
public sealed class MemberServiceIntegrationTests(
    PostgreSqlIntegrationFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task MemberLifecycle_ShouldPersistAcrossDatabaseContexts()
    {
        const ulong memberId = 123456789UL;

        var initialIdentity = new MemberIdentity(
            memberId,
            "InitialUser",
            "Initial User",
            DateTimeOffset.UtcNow,
            false);

        // Create
        var repository = new MemberRepository(Context);

        var service = new MemberService(
            repository,
            NullLogger<MemberService>.Instance);

        var creationStatus =
            await service.SynchronizeAsync(initialIdentity);

        Assert.Equal(
            MemberSynchronizationStatus.Created,
            creationStatus);

        var createdMember =
            await service.GetByIdAsync(memberId);

        Assert.NotNull(createdMember);

        var originalCreatedAt = createdMember.CreatedAt;

        await Context.DisposeAsync();

        // Synchronize from a fresh persistence context.
        Context = CreateDbContext();

        repository = new MemberRepository(Context);

        service = new MemberService(
            repository,
            NullLogger<MemberService>.Instance);

        var synchronizationStatus =
            await service.SynchronizeAsync(
                initialIdentity with
                {
                    Username = "UpdatedUser",
                    DisplayName = "Updated User"
                });

        Assert.Equal(
            MemberSynchronizationStatus.Updated,
            synchronizationStatus);

        await Context.DisposeAsync();

        // Deactivate from another fresh persistence context.
        Context = CreateDbContext();

        repository = new MemberRepository(Context);

        service = new MemberService(
            repository,
            NullLogger<MemberService>.Instance);

        var deactivationStatus =
            await service.DeactivateAsync(memberId);

        Assert.Equal(
            MemberDeactivationStatus.Deactivated,
            deactivationStatus);

        await Context.DisposeAsync();

        // Final persisted-state verification.
        Context = CreateDbContext();

        repository = new MemberRepository(Context);

        var persistedMember = await repository.GetByIdAsync(memberId);

        Assert.NotNull(persistedMember);

        var createdAtDifference = (persistedMember.CreatedAt - originalCreatedAt).Duration();

        Assert.True(createdAtDifference <= TimeSpan.FromMicroseconds(1),
            $"CreatedAt changed by {createdAtDifference}.");
        Assert.Equal("UpdatedUser", persistedMember.Username);
        Assert.Equal("Updated User", persistedMember.DisplayName);
        Assert.False(persistedMember.IsActive);
    }
}