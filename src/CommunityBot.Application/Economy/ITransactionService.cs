using CommunityBot.Core.Economy;

namespace CommunityBot.Application.Economy;

/// <summary>
/// Handles execution and history retrieval of financial movements.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Credits the requested positive amount and records the corresponding financial movement.
    /// </summary>
    Task AddBalanceAsync(TransactionRequest request, CancellationToken ct = default);

    /// <summary>
    /// Debits the requested positive amount and records the corresponding financial movement.
    /// </summary>
    Task SpendBalanceAsync(TransactionRequest request, CancellationToken ct = default);

    /// <summary>
    /// Returns the requested transaction history page and the total number of entries matching the filters.
    /// </summary>
    Task<(IEnumerable<Transaction> Transactions, int Count)> GetTransactionsHistoryAsync(
        MemberTransactionsHistory history,
        CancellationToken ct = default);
}