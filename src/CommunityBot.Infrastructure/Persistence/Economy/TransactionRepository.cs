using CommunityBot.Application.Economy;
using CommunityBot.Core.Economy;
using CommunityBot.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CommunityBot.Infrastructure.Persistence.Economy;

/// <summary>
/// Entity Framework Core implementation of financial transaction persistence.
/// </summary>
public sealed class TransactionRepository(AppDbContext context)
    : ABaseRepository<Transaction, long>(context), ITransactionRepository
{
    /// <inheritdoc />
    public async Task<(List<Transaction> Transactions, int Count)> GetHistoryAsync(
        MemberTransactionsHistory history,
        CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(transaction => transaction.MemberId == history.MemberId);

        if (history.FilterType.HasValue)
            query = query
                .Where(transaction => transaction.Type == history.FilterType.Value);

        var count = await query.CountAsync(ct);
        var transactions = await query
            .OrderByDescending(transaction => transaction.CreatedAt)
            .Skip(history.PageIndex * history.PageSize)
            .Take(history.PageSize)
            .ToListAsync(ct);

        return (transactions, count);
    }
}