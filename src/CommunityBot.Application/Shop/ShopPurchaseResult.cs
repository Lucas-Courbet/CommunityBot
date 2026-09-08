using CommunityBot.Application.Common.Results;

namespace CommunityBot.Application.Shop;

/// <summary>
/// Encapsulates the outcome of a shop purchase attempt.
/// </summary>
public sealed record PurchaseResult : Result
{
    public required PurchaseResultStatus Status { get; init; }

    public int? RemainingBalance { get; init; }

    public string? ItemLabel { get; init; }

    public int? PricePaid { get; init; }

    public long? GeneratedInventoryItemId { get; init; }

    public string? GrantedRoleKey { get; init; }

    public static PurchaseResult Success(
        int newBalance,
        string itemLabel,
        int pricePaid,
        long generatedInventoryItemId,
        string? grantedRoleKey = null)
        => new()
        {
            IsSuccess = true,
            Status = PurchaseResultStatus.Success,
            RemainingBalance = newBalance,
            ItemLabel = itemLabel,
            PricePaid = pricePaid,
            GeneratedInventoryItemId = generatedInventoryItemId,
            GrantedRoleKey = grantedRoleKey
        };

    public static PurchaseResult Failure(PurchaseResultStatus status, string message)
    {
        if (status == PurchaseResultStatus.Success)
            throw new ArgumentException(
                "A failed purchase result cannot use the Success status.",
                nameof(status));

        return new PurchaseResult
        {
            IsSuccess = false,
            Status = status,
            Message = message
        };
    }
}