using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Application.Goals;
using CommunityBot.Application.Items;
using CommunityBot.Application.Shop;
using CommunityBot.Core.Activities;
using CommunityBot.Core.Economy;
using CommunityBot.Core.Items;
using CommunityBot.Core.Members;
using CommunityBot.Infrastructure.Activities;
using CommunityBot.Infrastructure.Activities.Consumption;
using CommunityBot.Infrastructure.Persistence;
using CommunityBot.Infrastructure.Persistence.Activities;
using CommunityBot.Infrastructure.Persistence.Economy;
using CommunityBot.Infrastructure.Persistence.Goals;
using CommunityBot.Infrastructure.Persistence.Items;
using CommunityBot.Infrastructure.Persistence.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommunityBot.Tests.Integration.Goals;

[Collection(IntegrationTestCollection.Name)]
public sealed class CommunityGoalActivityIntegrationTests(
    PostgreSqlIntegrationFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task ShopPurchase_ShouldBeCapturedAndProgressCommunityGoal()
    {
        const ulong memberId = 6001UL;
        const string itemId = "community-badge";
        const string goalId = "shopping-drive";

        var now = DateTime.UtcNow;

        Context.Members.Add(CreateMember(memberId, 500));

        Context.ShopItems.Add(new ShopItem
        {
            Id = itemId,
            Item = new Item
            {
                Id = itemId,
                Label = "Community Badge",
                Description = "Integration test item",
                IsStackable = false
            },
            Price = 100,
            Category = ShopItemCategory.Collectible,
            IsEnabled = true
        });

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var goalService = CreateGoalService(Context);

        await goalService.CreateAsync(
            new CommunityGoalCreateRequest(
                goalId,
                "Community Shopping Drive",
                3,
                now.AddMinutes(-1),
                now.AddHours(1)));

        Context.ChangeTracker.Clear();

        var purchaseService = CreateShopPurchaseService(Context);
        var purchase = await purchaseService.PurchaseItemAsync(memberId, itemId);

        Assert.True(purchase.IsSuccess);

        await using (var capturedContext = CreateDbContext())
        {
            var transaction = await capturedContext.Transactions
                .SingleAsync(item =>
                    item.MemberId == memberId &&
                    item.Type == TransactionType.ShopPurchase);

            var activityEvent = await capturedContext.ActivityEvents.SingleAsync();
            var consumption = await capturedContext.ActivityConsumptions.SingleAsync();
            var goal = await capturedContext.CommunityGoals.SingleAsync(item => item.Id == goalId);

            Assert.Equal(ActivityEventType.ShopPurchaseCompleted, activityEvent.EventType);
            Assert.Equal(memberId, activityEvent.MemberId);
            Assert.Equal(1, activityEvent.OccurrenceCount);
            Assert.Equal($"shop-purchase:{transaction.Id}", activityEvent.SourceReference);

            Assert.Equal(ActivityConsumptionStatus.Pending, consumption.Status);
            Assert.Equal(0, goal.CurrentCount);
        }

        await using (var workerContext = CreateDbContext())
        {
            var processed = await CreateConsumptionService(workerContext)
                .ProcessNextAsync();

            Assert.True(processed);
        }

        await using var assertionContext = CreateDbContext();

        var persistedGoal = await assertionContext.CommunityGoals
            .SingleAsync(goal => goal.Id == goalId);

        var persistedConsumption = await assertionContext.ActivityConsumptions
            .SingleAsync();

        Assert.Equal(1, persistedGoal.CurrentCount);
        Assert.False(persistedGoal.IsCompleted);

        Assert.Equal(
            ActivityConsumptionStatus.Processed,
            persistedConsumption.Status);

        Assert.Equal(1, persistedConsumption.AttemptCount);
    }

    private static ICommunityGoalService CreateGoalService(AppDbContext context)
    {
        var gateRepository = new ActivityCaptureGateRepository(context);
        var subscriptionRepository = new ActivitySubscriptionRepository(context);

        return new CommunityGoalService(
            context,
            new CommunityGoalRepository(context),
            new ActivitySubscriptionPlanService(
                context,
                gateRepository,
                subscriptionRepository));
    }

    private static IShopPurchaseService CreateShopPurchaseService(AppDbContext context)
        => new ShopPurchaseService(
            new ShopPurchaseStore(
                new MemberRepository(context),
                new ShopItemRepository(context),
                new TransactionRepository(context)),
            context,
            new ItemAcquisitionService(
                new ItemRepository(context),
                new InventoryRepository(context)),
            CreateActivityCaptureService(context),
            NullLogger<ShopPurchaseService>.Instance);

    private static IActivityCaptureService CreateActivityCaptureService(AppDbContext context)
    {
        var gateRepository = new ActivityCaptureGateRepository(context);
        var subscriptionRepository = new ActivitySubscriptionRepository(context);
        var consumptionRepository = new ActivityConsumptionRepository(context);

        return new ActivityCaptureService(
            new ActivityCaptureStore(
                context,
                gateRepository,
                subscriptionRepository,
                new ActivityEventRepository(context),
                consumptionRepository,
                new ActivityCaptureIncidentRepository(context),
                new ActivityReconciliationRepository(context)),
            new ActivityCaptureTransactionCoordinator(context),
            NullLogger<ActivityCaptureService>.Instance);
    }

    private static IActivityConsumptionService CreateConsumptionService(AppDbContext context)
        => new ActivityConsumptionService(
            context,
            new ActivityConsumptionRepository(context),
            [
                new CommunityGoalActivityConsumer(
                    new CommunityGoalRepository(context))
            ],
            TimeProvider.System);

    private static Member CreateMember(ulong memberId, int balance)
    {
        var member = Member.Create(
            new MemberIdentity(
                memberId,
                $"User{memberId}",
                $"User {memberId}",
                null,
                false));

        member.CreditCurrency(balance);

        return member;
    }
}