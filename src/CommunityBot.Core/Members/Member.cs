using CommunityBot.Core.Common;
using CommunityBot.Core.Economy;
using CommunityBot.Core.Items;
using CommunityBot.Core.Rewards;

namespace CommunityBot.Core.Members;

/// <summary>
/// Represents a Discord member tracked by the application.
/// </summary>
public sealed class Member : IEntity<ulong>, IAuditable
{
    /// <summary>
    /// Unique Discord identifier of the member.
    /// </summary>
    public ulong Id { get; private set; }

    public string Username { get; private set; } = null!;

    public string? DisplayName { get; private set; }

    public DateTimeOffset? JoinedAt { get; private set; }

    public bool IsBot { get; private set; }

    /// <summary>
    /// Indicates whether the member currently belongs to the Discord community.
    /// Inactive records are retained so historical application data can be preserved.
    /// </summary>
    public bool IsActive { get; private set; }

    public int CurrencyBalance { get; private set; }

    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime UpdatedAt { get; set; }

    public ICollection<Transaction> Transactions { get; private set; }
        = new List<Transaction>();

    public ICollection<InventoryItem> Inventory { get; private set; }
        = new List<InventoryItem>();

    public ICollection<RewardEntitlement> RewardEntitlements { get; private set; }
        = new List<RewardEntitlement>();

    private Member()
    {
    }

    private Member(MemberIdentity identity)
    {
        Id = identity.Id;
        Username = identity.Username;
        DisplayName = identity.DisplayName;
        JoinedAt = NormalizeJoinedAt(identity.JoinedAt);
        IsBot = identity.IsBot;
        IsActive = true;
        CurrencyBalance = 0;
    }

    public static Member Create(MemberIdentity identity)
    {
        ValidateIdentity(identity);

        return new Member(identity);
    }

    /// <summary>
    /// Marks the member as inactive while preserving its persisted history.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Reactivates the member and synchronizes its current Discord identity.
    /// </summary>
    public void Reactivate(MemberIdentity identity)
    {
        SynchronizeIdentity(identity);
        IsActive = true;
    }

    public void CreditCurrency(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));

        CurrencyBalance += amount;
    }

    public void DebitCurrency(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));

        if (CurrencyBalance < amount)
            throw new InvalidOperationException("Insufficient funds.");

        CurrencyBalance -= amount;
    }

    /// <summary>
    /// Synchronizes the member with the supplied Discord identity.
    /// </summary>
    /// <remarks>
    /// Join timestamps are normalized to UTC. When Discord does not supply a join timestamp,
    /// an existing value is preserved rather than cleared.
    /// </remarks>
    /// <returns>
    /// <see langword="true"/> when at least one identity field changed;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool SynchronizeIdentity(MemberIdentity identity)
    {
        ValidateIdentity(identity);

        if (identity.Id != Id)
            throw new ArgumentException(
                "The supplied identity belongs to a different Discord member.",
                nameof(identity));

        var hasChanges = false;

        if (!string.Equals(Username, identity.Username, StringComparison.Ordinal))
        {
            Username = identity.Username;
            hasChanges = true;
        }

        if (!string.Equals(DisplayName, identity.DisplayName, StringComparison.Ordinal))
        {
            DisplayName = identity.DisplayName;
            hasChanges = true;
        }

        var joinedAt = NormalizeJoinedAt(identity.JoinedAt);

        if (joinedAt is not null && JoinedAt != joinedAt)
        {
            JoinedAt = joinedAt;
            hasChanges = true;
        }

        if (IsBot != identity.IsBot)
        {
            IsBot = identity.IsBot;
            hasChanges = true;
        }

        return hasChanges;
    }

    private static void ValidateIdentity(MemberIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        if (identity.Id == 0)
            throw new ArgumentException(
                "A Discord member identifier cannot be zero.",
                nameof(identity));

        if (string.IsNullOrWhiteSpace(identity.Username))
            throw new ArgumentException(
                "A Discord member username is required.",
                nameof(identity));
    }

    private static DateTimeOffset? NormalizeJoinedAt(DateTimeOffset? joinedAt)
        => joinedAt?.ToUniversalTime();
}