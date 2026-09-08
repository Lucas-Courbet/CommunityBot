namespace CommunityBot.Application.Shop;

/// <summary>
/// Categorizes the outcome of a shop purchase attempt.
/// </summary>
public enum PurchaseResultStatus
{
    Success,
    InsufficientFunds,
    ItemNotFound,
    ItemDisabled,
    AlreadyOwned,
    SystemError
}