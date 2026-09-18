using CommunityBot.Core.Common;
using CommunityBot.Core.Members;

namespace CommunityBot.Core.Economy;

/// <summary>
/// Represents an immutable persisted financial movement.
/// </summary>
public sealed class Transaction : IEntity<long>, IAuditable
{
    /// <inheritdoc />
    public long Id { get; init; }

    /// <summary>
    /// Discord identifier of the member affected by the transaction.
    /// </summary>
    public required ulong MemberId { get; init; }

    /// <summary>
    /// Affected member navigation property.
    /// </summary>
    public Member? Member { get; init; }

    /// <summary>
    /// Optional Discord identifier of the actor who initiated the transaction.
    /// </summary>
    public ulong? ActorId { get; init; }

    /// <summary>
    /// Signed amount represented by the transaction.
    /// </summary>
    public required int Amount { get; init; }

    /// <summary>
    /// Category of the financial movement.
    /// </summary>
    public required TransactionType Type { get; init; }

    /// <summary>
    /// Optional contextual reason associated with the transaction.
    /// </summary>
    public string? Reason { get; init; }

    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Creates a persisted financial movement.
    /// Positive amounts represent credits and negative amounts represent debits.
    /// </summary>
    public static Transaction Create(
        ulong memberId,
        int signedAmount,
        TransactionType type,
        string? reason,
        ulong? actorId = null)
        => new()
        {
            MemberId = memberId,
            ActorId = actorId,
            Amount = signedAmount,
            Type = type,
            Reason = reason
        };

    /// <summary>
    /// Creates the financial record associated with a shop purchase.
    /// </summary>
    public static Transaction CreateShopPurchase(ulong memberId, int price, string itemLabel)
        => Create(
            memberId,
            -price,
            TransactionType.ShopPurchase,
            $"Shop purchase: {itemLabel}");
}