using CommunityBot.Core.Members;
using CommunityBot.Infrastructure.Persistence.Members;

namespace CommunityBot.Tests.Integration;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuditInterceptorTests(
    PostgreSqlIntegrationFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task SaveChangesAsync_ShouldMaintainAuditTimestamps()
    {
        // Arrange
        var member = Member.Create(
            new MemberIdentity(
                999UL,
                "InitialName",
                null,
                null,
                false));

        var repository = new MemberRepository(Context);

        repository.Add(member);
        await repository.SaveChangesAsync();

        var initialCreatedAt = member.CreatedAt;
        var initialUpdatedAt = member.UpdatedAt;

        await Task.Delay(100);

        // Act
        member.SynchronizeIdentity(
            new MemberIdentity(
                member.Id,
                "ChangedName",
                null,
                null,
                false));

        await repository.SaveChangesAsync();

        // Assert
        Assert.NotEqual(default, member.CreatedAt);
        Assert.Equal(initialCreatedAt, member.CreatedAt);
        Assert.True(member.UpdatedAt > initialUpdatedAt);
    }
}