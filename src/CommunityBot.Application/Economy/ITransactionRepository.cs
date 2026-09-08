using CommunityBot.Application.Common.Persistence;
using CommunityBot.Core.Economy;

namespace CommunityBot.Application.Economy;

/// <summary>
/// Provides persistence operations for financial transactions.
/// </summary>
public interface ITransactionRepository : IBaseRepository<Transaction, long>
{
    Task<(List<Transaction> Transactions, int Count)> GetHistoryAsync(
        MemberTransactionsHistory history,
        CancellationToken ct = default);
}