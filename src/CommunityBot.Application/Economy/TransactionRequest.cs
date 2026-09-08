using CommunityBot.Core.Economy;

namespace CommunityBot.Application.Economy;

/// <summary>
/// Encapsulates the parameters required to execute a balance modification.
/// </summary>
public sealed record TransactionRequest(
    ulong MemberId,
    int Amount,
    TransactionType Type,
    string Reason,
    ulong? ActorId = null);