using CommunityBot.Core.Economy;

namespace CommunityBot.Application.Economy;

/// <summary>
/// Handles execution and history retrieval of financial movements.
/// </summary>
public interface ITransactionService
{
    Task AddBalanceAsync(TransactionRequest request, CancellationToken ct = default);

    Task SpendBalanceAsync(TransactionRequest request, CancellationToken ct = default);

    Task<(IEnumerable<Transaction> Transactions, int Count)> GetTransactionsHistoryAsync(
        MemberTransactionsHistory history,
        CancellationToken ct = default);
}