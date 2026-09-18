using CommunityBot.Application.Activities;
using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Application.Items;
using CommunityBot.Application.Members;
using CommunityBot.Application.Shop;
using CommunityBot.Core.Economy;
using CommunityBot.Core.Items;
using CommunityBot.Core.Members;
using CommunityBot.Infrastructure.Persistence;
using CommunityBot.Infrastructure.Persistence.Economy;
using CommunityBot.Infrastructure.Persistence.Items;
using CommunityBot.Infrastructure.Persistence.Members;
using CommunityBot.Tests.Integration.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommunityBot.Tests.Integration.Shop;

[Collection(IntegrationTestCollection.Name)]
public sealed class ShopPurchaseServiceIntegrationTests(PostgreSqlIntegrationFixture fixture)
    : IntegrationTestBase(fixture)
{
    private IShopPurchaseService _shopPurchaseService = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _shopPurchaseService = CreateShopPurchaseService(Context);
    }

    [Fact]
    public async Task PurchaseItemAsync_ShouldDebitMemberAndPersistInventoryAndTransaction()
    {
        const ulong memberId = 1001UL;
        const string itemId = "collectible-badge";
        const int initialBalance = 500;
        const int itemPrice = 200;

        Context.Members.Add(CreateMember(memberId, initialBalance));
        Context.ShopItems.Add(CreateShopItem(
            itemId,
            "Collectible Badge",
            itemPrice,
            ShopItemCategory.Collectible,
            isStackable: false));

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var result = await _shopPurchaseService.PurchaseItemAsync(memberId, itemId);

        await using var assertionContext = CreateDbContext();

        var member = await assertionContext.Members.SingleAsync(m => m.Id == memberId);
        var inventoryItem = await assertionContext.Inventory.SingleAsync(i =>
            i.MemberId == memberId && i.ItemId == itemId);
        var transaction = await assertionContext.Transactions.SingleAsync(t =>
            t.MemberId == memberId && t.Type == TransactionType.ShopPurchase);

        Assert.True(result.IsSuccess);
        Assert.Equal(PurchaseResultStatus.Success, result.Status);
        Assert.Equal(initialBalance - itemPrice, result.RemainingBalance);
        Assert.Equal("Collectible Badge", result.ItemLabel);
        Assert.Equal(itemPrice, result.PricePaid);
        Assert.Equal(inventoryItem.Id, result.GeneratedInventoryItemId);

        Assert.Equal(initialBalance - itemPrice, member.CurrencyBalance);
        Assert.Equal(ItemStatus.Active, inventoryItem.Status);
        Assert.Equal(1, inventoryItem.Quantity);
        Assert.Equal(-itemPrice, transaction.Amount);
        Assert.Equal("Shop purchase: Collectible Badge", transaction.Reason);
    }

    [Fact]
    public async Task PurchaseItemAsync_ShouldStackExistingInventory_WhenItemIsStackable()
    {
        const ulong memberId = 1002UL;
        const string itemId = "token-pack";
        const int initialBalance = 500;
        const int itemPrice = 100;

        Context.Members.Add(CreateMember(memberId, initialBalance));
        Context.ShopItems.Add(CreateShopItem(
            itemId,
            "Token Pack",
            itemPrice,
            ShopItemCategory.Consumable,
            isStackable: true));
        Context.Inventory.Add(InventoryItem.CreateActive(memberId, itemId, 2));

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var result = await _shopPurchaseService.PurchaseItemAsync(memberId, itemId);

        await using var assertionContext = CreateDbContext();

        var member = await assertionContext.Members.SingleAsync(m => m.Id == memberId);
        var inventoryItems = await assertionContext.Inventory
            .Where(i => i.MemberId == memberId && i.ItemId == itemId)
            .ToListAsync();
        var transactions = await assertionContext.Transactions
            .Where(t => t.MemberId == memberId && t.Type == TransactionType.ShopPurchase)
            .ToListAsync();

        Assert.Equal(PurchaseResultStatus.Success, result.Status);
        Assert.Equal(initialBalance - itemPrice, member.CurrencyBalance);

        var inventoryItem = Assert.Single(inventoryItems);
        Assert.Equal(3, inventoryItem.Quantity);

        var transaction = Assert.Single(transactions);
        Assert.Equal(-itemPrice, transaction.Amount);
    }

    [Fact]
    public async Task PurchaseItemAsync_ShouldRejectUniqueItemAlreadyOwnedWithoutMutation()
    {
        const ulong memberId = 1003UL;
        const string itemId = "unique-badge";
        const int initialBalance = 500;

        Context.Members.Add(CreateMember(memberId, initialBalance));
        Context.ShopItems.Add(CreateShopItem(
            itemId,
            "Unique Badge",
            200,
            ShopItemCategory.Collectible,
            isStackable: false));
        Context.Inventory.Add(InventoryItem.CreateActive(memberId, itemId));

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var result = await _shopPurchaseService.PurchaseItemAsync(memberId, itemId);

        await using var assertionContext = CreateDbContext();

        var member = await assertionContext.Members.SingleAsync(m => m.Id == memberId);
        var inventoryItems = await assertionContext.Inventory
            .Where(i => i.MemberId == memberId && i.ItemId == itemId)
            .ToListAsync();
        var hasTransaction = await assertionContext.Transactions.AnyAsync(t => t.MemberId == memberId);

        Assert.False(result.IsSuccess);
        Assert.Equal(PurchaseResultStatus.AlreadyOwned, result.Status);
        Assert.Equal(initialBalance, member.CurrencyBalance);

        var inventoryItem = Assert.Single(inventoryItems);
        Assert.Equal(1, inventoryItem.Quantity);
        Assert.False(hasTransaction);
    }

    [Fact]
    public async Task PurchaseItemAsync_ShouldNotPersistAnything_WhenFundsAreInsufficient()
    {
        const ulong memberId = 1004UL;
        const string itemId = "expensive-badge";
        const int initialBalance = 100;

        Context.Members.Add(CreateMember(memberId, initialBalance));
        Context.ShopItems.Add(CreateShopItem(
            itemId,
            "Expensive Badge",
            200,
            ShopItemCategory.Collectible,
            isStackable: false));

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var result = await _shopPurchaseService.PurchaseItemAsync(memberId, itemId);

        await using var assertionContext = CreateDbContext();

        var member = await assertionContext.Members.SingleAsync(m => m.Id == memberId);
        var hasInventory = await assertionContext.Inventory.AnyAsync(i => i.MemberId == memberId);
        var hasTransaction = await assertionContext.Transactions.AnyAsync(t => t.MemberId == memberId);

        Assert.False(result.IsSuccess);
        Assert.Equal(PurchaseResultStatus.InsufficientFunds, result.Status);
        Assert.Equal(initialBalance, member.CurrencyBalance);
        Assert.False(hasInventory);
        Assert.False(hasTransaction);
    }

    [Fact]
    public async Task PurchaseItemAsync_ShouldAllowOnlyOnePurchase_WhenSameUniqueItemIsPurchasedConcurrently()
    {
        const ulong memberId = 1005UL;
        const string itemId = "concurrent-badge";
        const int initialBalance = 500;
        const int itemPrice = 200;

        Context.Members.Add(CreateMember(memberId, initialBalance));
        Context.ShopItems.Add(CreateShopItem(
            itemId,
            "Concurrent Badge",
            itemPrice,
            ShopItemCategory.Collectible,
            isStackable: false));

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        await using var firstContext = CreateDbContext();
        await using var secondContext = CreateDbContext();

        var barrier = new AsyncBarrier(2);

        var firstService = CreateShopPurchaseService(
            firstContext,
            new SynchronizingMemberRepository(new MemberRepository(firstContext), barrier));

        var secondService = CreateShopPurchaseService(
            secondContext,
            new SynchronizingMemberRepository(new MemberRepository(secondContext), barrier));

        var results = await Task.WhenAll(
            firstService.PurchaseItemAsync(memberId, itemId),
            secondService.PurchaseItemAsync(memberId, itemId));

        await using var assertionContext = CreateDbContext();

        var member = await assertionContext.Members.SingleAsync(m => m.Id == memberId);
        var inventoryItems = await assertionContext.Inventory
            .Where(i => i.MemberId == memberId && i.ItemId == itemId)
            .ToListAsync();
        var transactions = await assertionContext.Transactions
            .Where(t => t.MemberId == memberId && t.Type == TransactionType.ShopPurchase)
            .ToListAsync();

        Assert.Equal(1, results.Count(result => result.IsSuccess));
        Assert.Equal(1, results.Count(result => !result.IsSuccess));
        Assert.Equal(
            PurchaseResultStatus.AlreadyOwned,
            results.Single(result => !result.IsSuccess).Status);

        Assert.Equal(initialBalance - itemPrice, member.CurrencyBalance);

        var inventoryItem = Assert.Single(inventoryItems);
        Assert.Equal(1, inventoryItem.Quantity);

        var transaction = Assert.Single(transactions);
        Assert.Equal(-itemPrice, transaction.Amount);
    }

    private static IShopPurchaseService CreateShopPurchaseService(
        AppDbContext context,
        IMemberRepository? memberRepository = null,
        IActivityCaptureService? activityCaptureService = null)
    {
        memberRepository ??= new MemberRepository(context);
        activityCaptureService ??= new NoOpActivityCaptureService();

        return new ShopPurchaseService(
            new ShopPurchaseStore(
                memberRepository,
                new ShopItemRepository(context),
                new TransactionRepository(context)),
            context,
            new ItemAcquisitionService(
                new ItemRepository(context),
                new InventoryRepository(context)),
            activityCaptureService,
            NullLogger<ShopPurchaseService>.Instance);
    }

    private static Member CreateMember(ulong memberId, int balance)
    {
        var member = Member.Create(
            new MemberIdentity(memberId, $"User{memberId}", $"User {memberId}", null, false));

        if (balance > 0)
            member.CreditCurrency(balance);

        return member;
    }

    private static ShopItem CreateShopItem(
        string id,
        string label,
        int price,
        ShopItemCategory category,
        bool isStackable,
        bool isEnabled = true)
        => new()
        {
            Id = id,
            Item = new Item
            {
                Id = id,
                Label = label,
                Description = "Integration test item",
                IsStackable = isStackable
            },
            Price = price,
            Category = category,
            IsEnabled = isEnabled
        };
    
    private sealed class NoOpActivityCaptureService : IActivityCaptureService
    {
        public Task<ActivityCaptureResult> CaptureAsync(
            ActivityEventCandidate candidate,
            CancellationToken ct = default)
            => Task.FromResult(
                new ActivityCaptureResult(
                    ActivityCaptureStatus.NotCaptured,
                    null));
    }
}