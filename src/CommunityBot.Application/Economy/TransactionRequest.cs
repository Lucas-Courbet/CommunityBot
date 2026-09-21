using CommunityBot.Core.Economy;

namespace CommunityBot.Application.Economy;

/// <summary>
/// Encapsulates a balance modification request using a positive amount whose financial direction is defined by the operation.
/// </summary>
public sealed record TransactionRequest(
    ulong MemberId,
    int Amount,
    TransactionType Type,
    string Reason,
    ulong? ActorId = null);