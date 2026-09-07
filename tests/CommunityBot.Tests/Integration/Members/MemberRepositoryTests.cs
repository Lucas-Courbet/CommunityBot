using CommunityBot.Core.Members;
using CommunityBot.Infrastructure.Persistence.Members;
using Microsoft.EntityFrameworkCore;

namespace CommunityBot.Tests.Integration.Members;

[Collection(IntegrationTestCollection.Name)]
public sealed class MemberRepositoryTests(
    PostgreSqlIntegrationFixture fixture)
    : IntegrationTestBase(fixture)
{
    private MemberRepository _repository = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _repository = new MemberRepository(Context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldRoundTripDiscordSnowflake()
    {
        // Arrange
        const ulong memberId = 1234567890123456789UL;

        var member = Member.Create(
            new MemberIdentity(
                memberId,
                "TestUser",
                "Test User",
                DateTimeOffset.UtcNow,
                false));

        _repository.Add(member);
        await _repository.SaveChangesAsync();

        Context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdAsync(memberId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(memberId, result.Id);
        Assert.Equal("TestUser", result.Username);
        Assert.Equal("Test User", result.DisplayName);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnMemberWithoutTracking()
    {
        // Arrange
        var member = Member.Create(
            new MemberIdentity(
                456UL,
                "LookupUser",
                null,
                null,
                false));

        _repository.Add(member);
        await _repository.SaveChangesAsync();

        Context.ChangeTracker.Clear();

        // Act
        var result =
            await _repository.GetByUsernameAsync("LookupUser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(member.Id, result.Id);
        Assert.Equal(
            EntityState.Detached,
            Context.Entry(result).State);
    }
}