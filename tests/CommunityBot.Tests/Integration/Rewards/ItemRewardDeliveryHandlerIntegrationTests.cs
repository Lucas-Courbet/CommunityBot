using CommunityBot.Application.Items;
using CommunityBot.Application.Rewards;
using CommunityBot.Core.Items;
using CommunityBot.Core.Members;
using CommunityBot.Core.Rewards;
using CommunityBot.Infrastructure.Persistence;
using CommunityBot.Infrastructure.Persistence.Items;
using CommunityBot.Infrastructure.Persistence.Members;
using CommunityBot.Infrastructure.Persistence.Rewards;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommunityBot.Tests.Integration.Rewards;

[Collection(IntegrationTestCollection.Name)]
public sealed class ItemRewardDeliveryHandlerIntegrationTests(
    PostgreSqlIntegrationFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task DeliverAsync_ShouldAddItemToInventoryAndFinalizeEntitlement()
    {
        const ulong memberId = 4001UL;
        const string itemId = "reward-token";
        const int quantity = 3;

        var member = Member.Create(
            new MemberIdentity(
                memberId,
                "RewardUser",
                "Reward User",
                null,
                false));

        var item = new Item
        {
            Id = itemId,
            Label = "Reward Token",
            IsStackable = true
        };

        var entitlement = new RewardEntitlement
        {
            MemberId = memberId,
            SourceReference = "item-reward",
            RewardType = RewardType.Item,
            RewardReference = itemId,
            Quantity = quantity
        };

        Context.Members.Add(member);
        Context.Items.Add(item);
        Context.RewardEntitlements.Add(entitlement);

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var handler = CreateHandler(Context);

        var result = await handler.DeliverAsync(entitlement.Id);

        await using var assertionContext = CreateDbContext();

        var persistedEntitlement = await assertionContext.RewardEntitlements
            .SingleAsync(e => e.Id == entitlement.Id);

        var inventoryItem = await assertionContext.Inventory.SingleAsync(i =>
            i.MemberId == memberId &&
            i.ItemId == itemId);

        Assert.Equal(RewardDeliveryStatus.Delivered, result.Status);
        Assert.Equal(RewardEntitlementStatus.Delivered, persistedEntitlement.Status);
        Assert.Equal(1, persistedEntitlement.AttemptCount);

        Assert.Equal(ItemStatus.Active, inventoryItem.Status);
        Assert.Equal(quantity, inventoryItem.Quantity);
    }

    private static ItemRewardDeliveryHandler CreateHandler(AppDbContext context)
    {
        var itemRepository = new ItemRepository(context);

        return new ItemRewardDeliveryHandler(
            context,
            new RewardEntitlementRepository(context),
            itemRepository,
            new MemberRepository(context),
            new ItemAcquisitionService(
                itemRepository,
                new InventoryRepository(context)),
            NullLogger<ItemRewardDeliveryHandler>.Instance);
    }
}