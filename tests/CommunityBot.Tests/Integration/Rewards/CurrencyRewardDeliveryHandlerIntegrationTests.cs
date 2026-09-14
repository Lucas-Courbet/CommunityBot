using CommunityBot.Application.Rewards;
using CommunityBot.Core.Economy;
using CommunityBot.Core.Members;
using CommunityBot.Core.Rewards;
using CommunityBot.Infrastructure.Persistence;
using CommunityBot.Infrastructure.Persistence.Economy;
using CommunityBot.Infrastructure.Persistence.Members;
using CommunityBot.Infrastructure.Persistence.Rewards;
using CommunityBot.Tests.Integration.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommunityBot.Tests.Integration.Rewards;

[Collection(IntegrationTestCollection.Name)]
public sealed class CurrencyRewardDeliveryHandlerIntegrationTests(
    PostgreSqlIntegrationFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task DeliverAsync_ShouldCreditMemberAndFinalizeEntitlementAtomically()
    {
        const ulong memberId = 3001UL;
        const int initialBalance = 100;
        const int reward = 50;

        var entitlementId = await SeedCurrencyEntitlementAsync(
            memberId,
            initialBalance,
            reward);

        var handler = CreateHandler(Context);

        var result = await handler.DeliverAsync(entitlementId);

        await using var assertionContext = CreateDbContext();

        var member = await assertionContext.Members.SingleAsync(m => m.Id == memberId);
        var entitlement = await assertionContext.RewardEntitlements
            .SingleAsync(e => e.Id == entitlementId);
        var transaction = await assertionContext.Transactions
            .SingleAsync(t => t.MemberId == memberId && t.Type == TransactionType.Reward);

        Assert.Equal(RewardDeliveryStatus.Delivered, result.Status);
        Assert.Equal(initialBalance + reward, member.CurrencyBalance);

        Assert.Equal(RewardEntitlementStatus.Delivered, entitlement.Status);
        Assert.Equal(1, entitlement.AttemptCount);
        Assert.NotNull(entitlement.DeliveredAt);

        Assert.Equal(reward, transaction.Amount);
        Assert.Equal("Reward delivery", transaction.Reason);
    }

    [Fact]
    public async Task DeliverAsync_ShouldCreditOnlyOnce_WhenSameEntitlementIsDeliveredConcurrently()
    {
        const ulong memberId = 3002UL;
        const int initialBalance = 100;
        const int reward = 75;

        var entitlementId = await SeedCurrencyEntitlementAsync(
            memberId,
            initialBalance,
            reward);

        await using var firstContext = CreateDbContext();
        await using var secondContext = CreateDbContext();

        var barrier = new AsyncBarrier(2);

        var firstHandler = CreateHandler(
            firstContext,
            new SynchronizingRewardEntitlementRepository(
                new RewardEntitlementRepository(firstContext),
                barrier));

        var secondHandler = CreateHandler(
            secondContext,
            new SynchronizingRewardEntitlementRepository(
                new RewardEntitlementRepository(secondContext),
                barrier));

        var results = await Task.WhenAll(
            firstHandler.DeliverAsync(entitlementId),
            secondHandler.DeliverAsync(entitlementId));

        await using var assertionContext = CreateDbContext();

        var member = await assertionContext.Members.SingleAsync(m => m.Id == memberId);
        var entitlement = await assertionContext.RewardEntitlements
            .SingleAsync(e => e.Id == entitlementId);
        var transactions = await assertionContext.Transactions
            .Where(t => t.MemberId == memberId && t.Type == TransactionType.Reward)
            .ToListAsync();

        Assert.Equal(
            1,
            results.Count(result => result.Status == RewardDeliveryStatus.Delivered));

        Assert.Equal(
            1,
            results.Count(result => result.Status == RewardDeliveryStatus.AlreadyFinalized));

        Assert.Equal(initialBalance + reward, member.CurrencyBalance);
        Assert.Equal(1, entitlement.AttemptCount);

        var transaction = Assert.Single(transactions);
        Assert.Equal(reward, transaction.Amount);
    }

    private async Task<long> SeedCurrencyEntitlementAsync(
        ulong memberId,
        int initialBalance,
        int quantity)
    {
        var member = Member.Create(
            new MemberIdentity(
                memberId,
                $"User{memberId}",
                $"User {memberId}",
                null,
                false));

        if (initialBalance > 0)
            member.CreditCurrency(initialBalance);

        var entitlement = new RewardEntitlement
        {
            MemberId = memberId,
            SourceReference = $"currency-reward-{memberId}",
            RewardType = RewardType.Currency,
            Quantity = quantity
        };

        Context.Members.Add(member);
        Context.RewardEntitlements.Add(entitlement);

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        return entitlement.Id;
    }

    private static CurrencyRewardDeliveryHandler CreateHandler(
        AppDbContext context,
        IRewardEntitlementRepository? rewardEntitlementRepository = null)
        => new(
            context,
            rewardEntitlementRepository ?? new RewardEntitlementRepository(context),
            new MemberRepository(context),
            new TransactionRepository(context),
            NullLogger<CurrencyRewardDeliveryHandler>.Instance);
}