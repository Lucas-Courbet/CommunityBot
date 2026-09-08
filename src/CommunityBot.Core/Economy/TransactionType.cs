namespace CommunityBot.Core.Economy;

/// <summary>
/// Categorizes persisted financial transactions.
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Currency granted by an application reward.
    /// </summary>
    Reward,

    /// <summary>
    /// Currency spent through the application shop.
    /// </summary>
    ShopPurchase,

    /// <summary>
    /// Administrative balance adjustment.
    /// </summary>
    Admin,

    /// <summary>
    /// Currency returned after a previous expenditure.
    /// </summary>
    Refund
}