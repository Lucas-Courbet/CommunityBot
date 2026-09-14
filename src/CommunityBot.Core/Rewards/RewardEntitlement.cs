using CommunityBot.Core.Common;
using CommunityBot.Core.Members;

namespace CommunityBot.Core.Rewards;

/// <summary>
/// Represents a reward owed to a member and tracks its delivery lifecycle.
/// </summary>
public sealed class RewardEntitlement : IEntity<long>, IAuditable
{
    public long Id { get; init; }

    public required ulong MemberId { get; init; }

    /// <summary>
    /// Stable identifier of the operation that created this entitlement.
    /// </summary>
    public required string SourceReference { get; init; }

    public required RewardType RewardType { get; init; }

    /// <summary>
    /// Optional target reference used by reward-specific delivery handlers.
    /// </summary>
    public string? RewardReference { get; init; }

    public required int Quantity { get; init; }

    public RewardEntitlementStatus Status { get; private set; } = RewardEntitlementStatus.Pending;

    public int AttemptCount { get; private set; }

    public DateTime? LastAttemptAt { get; private set; }

    public string? LastError { get; private set; }

    public DateTime? DeliveredAt { get; private set; }

    public Member? Member { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsFinalized => Status == RewardEntitlementStatus.Delivered;

    public void MarkAsDelivered()
    {
        EnsureNotFinalized();

        var now = DateTime.UtcNow;

        AttemptCount++;
        LastAttemptAt = now;
        Status = RewardEntitlementStatus.Delivered;
        DeliveredAt = now;
    }

    public void RecordDeliveryFailure(string error)
    {
        EnsureNotFinalized();

        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Delivery error cannot be empty.", nameof(error));

        AttemptCount++;
        LastAttemptAt = DateTime.UtcNow;
        LastError = error;
        Status = RewardEntitlementStatus.Error;
    }

    private void EnsureNotFinalized()
    {
        if (IsFinalized)
        {
            throw new InvalidOperationException(
                $"Reward entitlement {Id} is already finalized with status {Status}.");
        }
    }
}