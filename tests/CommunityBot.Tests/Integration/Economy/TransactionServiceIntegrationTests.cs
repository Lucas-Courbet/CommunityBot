using CommunityBot.Application.Economy;
using CommunityBot.Application.Members;
using CommunityBot.Core.Economy;
using CommunityBot.Core.Members;
using CommunityBot.Infrastructure.Persistence;
using CommunityBot.Infrastructure.Persistence.Economy;
using CommunityBot.Infrastructure.Persistence.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommunityBot.Tests.Integration.Economy;

[Collection(IntegrationTestCollection.Name)]
public sealed class TransactionServiceIntegrationTests(PostgreSqlIntegrationFixture fixture)
    : IntegrationTestBase(fixture)
{
    private ITransactionService _transactionService = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _transactionService = CreateTransactionService(Context, new MemberRepository(Context));
    }

    [Fact]
    public async Task AddBalanceAsync_ShouldCreditMemberAndPersistTransaction()
    {
        const ulong memberId = 100UL;
        const ulong actorId = 999UL;

        await AddMemberAsync(memberId);

        var request = new TransactionRequest(
            memberId,
            150,
            TransactionType.Admin,
            "Manual adjustment",
            actorId);

        await _transactionService.AddBalanceAsync(request);

        Context.ChangeTracker.Clear();

        var member = await Context.Members.SingleAsync(m => m.Id == memberId);
        var transaction = await Context.Transactions.SingleAsync(t => t.MemberId == memberId);

        Assert.Equal(150, member.CurrencyBalance);
        Assert.Equal(150, transaction.Amount);
        Assert.Equal(TransactionType.Admin, transaction.Type);
        Assert.Equal("Manual adjustment", transaction.Reason);
        Assert.Equal(actorId, transaction.ActorId);
    }

    [Fact]
    public async Task SpendBalanceAsync_ShouldDebitMemberAndPersistNegativeTransaction()
    {
        const ulong memberId = 200UL;

        await AddMemberAsync(memberId, 500);

        var request = new TransactionRequest(
            memberId,
            200,
            TransactionType.ShopPurchase,
            "Shop purchase");

        await _transactionService.SpendBalanceAsync(request);

        Context.ChangeTracker.Clear();

        var member = await Context.Members.SingleAsync(m => m.Id == memberId);
        var transaction = await Context.Transactions.SingleAsync(t => t.MemberId == memberId);

        Assert.Equal(300, member.CurrencyBalance);
        Assert.Equal(-200, transaction.Amount);
        Assert.Equal(TransactionType.ShopPurchase, transaction.Type);
    }

    [Fact]
    public async Task SpendBalanceAsync_ShouldRollback_WhenFundsAreInsufficient()
    {
        const ulong memberId = 300UL;

        await AddMemberAsync(memberId, 50);

        var request = new TransactionRequest(
            memberId,
            100,
            TransactionType.ShopPurchase,
            "Too expensive");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _transactionService.SpendBalanceAsync(request));

        Context.ChangeTracker.Clear();

        var member = await Context.Members.SingleAsync(m => m.Id == memberId);
        var transactions = await Context.Transactions.Where(t => t.MemberId == memberId).ToListAsync();

        Assert.Equal(50, member.CurrencyBalance);
        Assert.Empty(transactions);
    }

    [Fact]
    public async Task SpendBalanceAsync_ShouldAllowOnlyOneDebit_WhenConcurrentSpendsExceedBalance()
    {
        const ulong memberId = 400UL;
        const int initialBalance = 300;
        const int spendAmount = 200;

        await AddMemberAsync(memberId, initialBalance);

        await using var firstContext = CreateDbContext();
        await using var secondContext = CreateDbContext();

        var barrier = new AsyncBarrier(2);

        var firstService = CreateTransactionService(
            firstContext,
            new SynchronizingMemberRepository(new MemberRepository(firstContext), barrier));

        var secondService = CreateTransactionService(
            secondContext,
            new SynchronizingMemberRepository(new MemberRepository(secondContext), barrier));

        var request = new TransactionRequest(
            memberId,
            spendAmount,
            TransactionType.Admin,
            "Concurrent spend");

        var outcomes = await Task.WhenAll(
            CaptureExceptionAsync(() => firstService.SpendBalanceAsync(request)),
            CaptureExceptionAsync(() => secondService.SpendBalanceAsync(request)));

        await using var assertionContext = CreateDbContext();

        var member = await assertionContext.Members.SingleAsync(m => m.Id == memberId);
        var transactions = await assertionContext.Transactions
            .Where(t => t.MemberId == memberId && t.Type == TransactionType.Admin)
            .ToListAsync();

        Assert.Equal(1, outcomes.Count(ex => ex is null));
        Assert.Equal(1, outcomes.Count(ex => ex is InvalidOperationException));
        Assert.Equal(100, member.CurrencyBalance);

        var transaction = Assert.Single(transactions);
        Assert.Equal(-spendAmount, transaction.Amount);
        Assert.Equal("Concurrent spend", transaction.Reason);
    }

    [Fact]
    public async Task GetTransactionsHistoryAsync_ShouldFilterAndPaginateHistory()
    {
        const ulong memberId = 500UL;

        await AddMemberAsync(memberId);

        await _transactionService.AddBalanceAsync(
            new TransactionRequest(memberId, 10, TransactionType.Reward, "First reward"));

        await _transactionService.AddBalanceAsync(
            new TransactionRequest(memberId, 20, TransactionType.Admin, "Adjustment"));

        await _transactionService.AddBalanceAsync(
            new TransactionRequest(memberId, 30, TransactionType.Reward, "Second reward"));

        var history = new MemberTransactionsHistory(
            memberId,
            PageIndex: 0,
            PageSize: 1,
            FilterType: TransactionType.Reward);

        var (transactions, count) = await _transactionService.GetTransactionsHistoryAsync(history);

        Assert.Equal(2, count);

        var transaction = Assert.Single(transactions);
        Assert.Equal(TransactionType.Reward, transaction.Type);
    }

    private async Task AddMemberAsync(ulong id, int balance = 0)
    {
        var member = Member.Create(new MemberIdentity(id, $"User{id}", $"User {id}", null, false));

        if (balance > 0)
            member.CreditCurrency(balance);

        Context.Members.Add(member);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    private static ITransactionService CreateTransactionService(
        AppDbContext context,
        IMemberRepository memberRepository)
        => new TransactionService(
            context,
            new TransactionRepository(context),
            memberRepository,
            NullLogger<TransactionService>.Instance);

    private static async Task<Exception?> CaptureExceptionAsync(Func<Task> action)
    {
        try
        {
            await action();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    private sealed class SynchronizingMemberRepository(
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

    private sealed class AsyncBarrier(int participantCount)
    {
        private readonly object _lock = new();
        private readonly TaskCompletionSource _release =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private int _remaining = participantCount;

        public async Task SignalAndWaitAsync(CancellationToken ct)
        {
            Task releaseTask;

            lock (_lock)
            {
                _remaining--;

                if (_remaining == 0)
                    _release.TrySetResult();

                releaseTask = _release.Task;
            }

            await releaseTask.WaitAsync(TimeSpan.FromSeconds(10), ct);
        }
    }
}