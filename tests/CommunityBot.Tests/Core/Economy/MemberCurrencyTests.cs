using CommunityBot.Core.Members;

namespace CommunityBot.Tests.Core.Economy;

public sealed class MemberCurrencyTests
{
    [Fact]
    public void CreditCurrency_WithPositiveAmount_IncreasesBalance()
    {
        var member = CreateMember();

        member.CreditCurrency(150);

        Assert.Equal(150, member.CurrencyBalance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreditCurrency_WithNonPositiveAmount_Throws(int amount)
    {
        var member = CreateMember();

        Assert.Throws<ArgumentException>(() => member.CreditCurrency(amount));
    }

    [Fact]
    public void DebitCurrency_WithAvailableFunds_DecreasesBalance()
    {
        var member = CreateMember();
        member.CreditCurrency(200);

        member.DebitCurrency(75);

        Assert.Equal(125, member.CurrencyBalance);
    }

    [Fact]
    public void DebitCurrency_WithInsufficientFunds_ThrowsWithoutChangingBalance()
    {
        var member = CreateMember();
        member.CreditCurrency(50);

        Assert.Throws<InvalidOperationException>(() => member.DebitCurrency(100));
        Assert.Equal(50, member.CurrencyBalance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DebitCurrency_WithNonPositiveAmount_Throws(int amount)
    {
        var member = CreateMember();
        member.CreditCurrency(100);

        Assert.Throws<ArgumentException>(() => member.DebitCurrency(amount));
        Assert.Equal(100, member.CurrencyBalance);
    }

    private static Member CreateMember()
        => Member.Create(new MemberIdentity(123UL, "TestUser", null, null, false));
}