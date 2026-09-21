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
    /// Loads the state required to evaluate a purchase while locking the member row
    /// for the caller-owned transaction.
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

    public Transaction AddPurchaseTransaction(Member member, ShopItem shopItem)
    {
        var transaction = Transaction.CreateShopPurchase(
            member.Id,
            shopItem.Price,
            shopItem.Item.Label);

        transactionRepository.Add(transaction);

        return transaction;
    }
}