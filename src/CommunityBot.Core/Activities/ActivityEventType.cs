namespace CommunityBot.Core.Activities;

/// <summary>
/// Identifies stable activity facts that can be captured durably.
/// Persisted values form part of the activity event contract and must not be renamed casually.
/// </summary>
public enum ActivityEventType
{
    ShopPurchaseCompleted
}