namespace CommunityBot.Core.Economy;

/// <summary>
/// Categorizes persisted financial transactions.
/// </summary>
public enum TransactionType
{
    Reward,
    ShopPurchase,
    Admin,
    Refund
}