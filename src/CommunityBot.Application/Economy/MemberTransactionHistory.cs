using CommunityBot.Core.Economy;

namespace CommunityBot.Application.Economy;

/// <summary>
/// Defines criteria for filtering and paginating a member's transaction history.
/// </summary>
public sealed record MemberTransactionsHistory(
    ulong MemberId,
    int PageIndex,
    int PageSize,
    TransactionType? FilterType = null);