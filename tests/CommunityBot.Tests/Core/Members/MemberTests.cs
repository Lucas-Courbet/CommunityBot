using CommunityBot.Core.Members;

namespace CommunityBot.Tests.Core.Members;

public sealed class MemberTests
{
    [Fact]
    public void Create_WithValidIdentity_CreatesActiveMember()
    {
        var joinedAt = new DateTimeOffset(
            2026, 1, 15, 18, 30, 0,
            TimeSpan.FromHours(1));

        var identity = new MemberIdentity(
            123,
            "lucas",
            "Lucas",
            joinedAt,
            false);

        var member = Member.Create(identity);

        Assert.Equal(123UL, member.Id);
        Assert.Equal("lucas", member.Username);
        Assert.Equal("Lucas", member.DisplayName);
        Assert.Equal(joinedAt.ToUniversalTime(), member.JoinedAt);
        Assert.False(member.IsBot);
        Assert.True(member.IsActive);
    }

    [Fact]
    public void Create_WithZeroId_ThrowsArgumentException()
    {
        var identity = new MemberIdentity(
            0,
            "lucas",
            null,
            null,
            false);

        Assert.Throws<ArgumentException>(() => Member.Create(identity));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_WithInvalidUsername_ThrowsArgumentException(string username)
    {
        var identity = new MemberIdentity(
            123,
            username,
            null,
            null,
            false);

        Assert.Throws<ArgumentException>(() => Member.Create(identity));
    }

    [Fact]
    public void Deactivate_ActiveMember_MarksMemberAsInactive()
    {
        var member = CreateMember();

        member.Deactivate();

        Assert.False(member.IsActive);
    }

    [Fact]
    public void Reactivate_InactiveMember_UpdatesIdentityAndMarksMemberAsActive()
    {
        var member = CreateMember();
        member.Deactivate();

        var identity = new MemberIdentity(
            member.Id,
            "updated-user",
            "Updated display name",
            new DateTimeOffset(2026, 2, 10, 12, 0, 0, TimeSpan.Zero),
            true);

        member.Reactivate(identity);

        Assert.True(member.IsActive);
        Assert.Equal("updated-user", member.Username);
        Assert.Equal("Updated display name", member.DisplayName);
        Assert.Equal(identity.JoinedAt, member.JoinedAt);
        Assert.True(member.IsBot);
    }

    [Fact]
    public void SynchronizeIdentity_WithIdenticalIdentity_ReturnsFalse()
    {
        var identity = CreateIdentity();
        var member = Member.Create(identity);

        var changed = member.SynchronizeIdentity(identity);

        Assert.False(changed);
    }

    [Fact]
    public void SynchronizeIdentity_WithChangedIdentity_UpdatesMemberAndReturnsTrue()
    {
        var member = CreateMember();

        var identity = new MemberIdentity(
            member.Id,
            "new-username",
            "New display name",
            new DateTimeOffset(2026, 3, 1, 14, 0, 0, TimeSpan.Zero),
            true);

        var changed = member.SynchronizeIdentity(identity);

        Assert.True(changed);
        Assert.Equal("new-username", member.Username);
        Assert.Equal("New display name", member.DisplayName);
        Assert.Equal(identity.JoinedAt, member.JoinedAt);
        Assert.True(member.IsBot);
    }

    [Fact]
    public void SynchronizeIdentity_WithMissingJoinedAt_PreservesKnownJoinedAt()
    {
        var originalIdentity = CreateIdentity();
        var member = Member.Create(originalIdentity);

        var updatedIdentity = originalIdentity with
        {
            Username = "new-username",
            JoinedAt = null
        };

        var changed = member.SynchronizeIdentity(updatedIdentity);

        Assert.True(changed);
        Assert.Equal(originalIdentity.JoinedAt?.ToUniversalTime(), member.JoinedAt);
    }

    [Fact]
    public void SynchronizeIdentity_WithDifferentId_ThrowsArgumentException()
    {
        var member = CreateMember();

        var identity = CreateIdentity() with
        {
            Id = member.Id + 1
        };

        Assert.Throws<ArgumentException>(
            () => member.SynchronizeIdentity(identity));
    }

    private static Member CreateMember()
        => Member.Create(CreateIdentity());

    private static MemberIdentity CreateIdentity()
        => new(
            123,
            "lucas",
            "Lucas",
            new DateTimeOffset(2026, 1, 15, 18, 30, 0, TimeSpan.Zero),
            false);
}