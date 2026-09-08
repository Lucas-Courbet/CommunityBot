using CommunityBot.Application.Economy;
using CommunityBot.Application.Items;
using CommunityBot.Application.Members;
using CommunityBot.Core.Economy;
using CommunityBot.Core.Items;
using CommunityBot.Core.Members;

namespace CommunityBot.Application.Shop;

/// <summary>
/// Provides the persistence operations required by the shop purchase workflow.
/// Transaction ownership and business decisions remain the responsibility of
/// <see cref="ShopPurchaseService"/>.
/// </summary>
public sealed class ShopPurchaseStore(
    IMemberRepository memberRepository,
    IShopItemRepository shopItemRepository,
    ITransactionRepository transactionRepository)
{
    /// <summary>
    /// Loads the persistent state required to evaluate a purchase.
    /// The member row is locked for the caller-owned transaction.
    /// </summary>
    public async Task<(Member? Member, ShopItem? ShopItem)> LoadPurchaseContextAsync(
        ulong memberId,
        string itemId,
        CancellationToken ct = default)
    {
        var member = await memberRepository.GetByIdForUpdateAsync(memberId, ct);
        var shopItem = await shopItemRepository.GetWithItemAsync(itemId, ct);

        return (member, shopItem);
    }

    /// <summary>
    /// Registers the financial transaction associated with a shop purchase.
    /// </summary>
    public void AddPurchaseTransaction(Member member, ShopItem shopItem)
    {
        transactionRepository.Add(
            Transaction.CreateShopPurchase(
                member.Id,
                shopItem.Price,
                shopItem.Item.Label));
    }
}