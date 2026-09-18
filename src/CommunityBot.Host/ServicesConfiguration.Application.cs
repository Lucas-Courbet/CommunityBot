using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Application.Economy;
using CommunityBot.Application.Goals;
using CommunityBot.Application.Items;
using CommunityBot.Application.Members;
using CommunityBot.Application.Rewards;
using CommunityBot.Application.Shop;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityBot.Host;

public static partial class ServicesConfiguration
{
    /// <summary>
    /// Registers application services and use-case orchestrators.
    /// </summary>
    private static void ConfigureApplicationServices(IServiceCollection services)
    {
        // Economy
        services.AddScoped<ITransactionService, TransactionService>();

        // Items
        services.AddScoped<IItemAcquisitionService, ItemAcquisitionService>();

        // Members
        services.AddScoped<IMemberService, MemberService>();

        // Shop
        services.AddScoped<ShopPurchaseStore>();
        services.AddScoped<IShopCatalogService, ShopCatalogService>();
        services.AddScoped<IShopPurchaseService, ShopPurchaseService>();

        // Rewards
        services.AddScoped<IRewardDeliveryService, RewardDeliveryService>();
        services.AddScoped<IRewardDeliveryHandler, CurrencyRewardDeliveryHandler>();
        services.AddScoped<IRewardDeliveryHandler, ItemRewardDeliveryHandler>();
        
        // Goals
        services.AddScoped<ICommunityGoalService, CommunityGoalService>();
        services.AddScoped<IActivityConsumer, CommunityGoalActivityConsumer>();
    }
}