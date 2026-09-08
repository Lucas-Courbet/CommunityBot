using CommunityBot.Application.Common.Persistence;
using CommunityBot.Application.Members;
using CommunityBot.Core.Economy;
using CommunityBot.Core.Members;
using Microsoft.Extensions.Logging;

namespace CommunityBot.Application.Economy;

/// <summary>
/// Handles persisted financial movements and member balance updates.
/// </summary>
/// <remarks>
/// Balance mutations and their associated financial records are persisted
/// atomically inside the same database transaction.
/// </remarks>
public sealed class TransactionService(
    IPersistenceContext persistenceContext,
    ITransactionRepository transactionRepository,
    IMemberRepository memberRepository,
    ILogger<TransactionService> logger)
    : ITransactionService
{
    /// <inheritdoc />
    public async Task AddBalanceAsync(TransactionRequest request, CancellationToken ct = default)
        => await ProcessTransactionAsync(request, isSpending: false, ct);

    /// <inheritdoc />
    public async Task SpendBalanceAsync(TransactionRequest request, CancellationToken ct = default)
        => await ProcessTransactionAsync(request, isSpending: true, ct);

    /// <inheritdoc />
    public async Task<(IEnumerable<Transaction> Transactions, int Count)> GetTransactionsHistoryAsync(
        MemberTransactionsHistory history,
        CancellationToken ct = default)
        => await transactionRepository.GetHistoryAsync(history, ct);

    private async Task ProcessTransactionAsync(
        TransactionRequest request,
        bool isSpending,
        CancellationToken ct)
    {
        if (request.Amount <= 0)
            throw new ArgumentException("Transaction amount must be positive.", nameof(request));

        await using var transaction = await persistenceContext.BeginTransactionAsync(ct);

        try
        {
            var member = await memberRepository.GetByIdForUpdateAsync(request.MemberId, ct)
                         ?? throw new InvalidOperationException(
                             $"Member with ID {request.MemberId} was not found.");

            ApplyBalanceChange(member, request.Amount, isSpending);

            transactionRepository.Add(CreateTransaction(request, isSpending));

            await persistenceContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            logger.LogInformation(
                "Transaction successful: {Type} of {Amount} for {MemberId}",
                request.Type,
                request.Amount,
                request.MemberId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);

            logger.LogError(
                ex,
                "Transaction failed for {MemberId}. Type: {Type}",
                request.MemberId,
                request.Type);

            throw;
        }
    }

    private static Transaction CreateTransaction(TransactionRequest request, bool isSpending)
        => Transaction.Create(
            request.MemberId,
            isSpending ? -request.Amount : request.Amount,
            request.Type,
            request.Reason,
            request.ActorId);

    private static void ApplyBalanceChange(Member member, int amount, bool isSpending)
    {
        if (isSpending)
        {
            member.DebitCurrency(amount);
            return;
        }

        member.CreditCurrency(amount);
    }
}