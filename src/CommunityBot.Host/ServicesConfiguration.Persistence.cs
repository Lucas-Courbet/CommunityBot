using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Application.Common.Persistence;
using CommunityBot.Application.Economy;
using CommunityBot.Application.Goals;
using CommunityBot.Application.Items;
using CommunityBot.Application.Members;
using CommunityBot.Application.Rewards;
using CommunityBot.Infrastructure.Persistence;
using CommunityBot.Infrastructure.Persistence.Activities;
using CommunityBot.Infrastructure.Persistence.Economy;
using CommunityBot.Infrastructure.Persistence.Goals;
using CommunityBot.Infrastructure.Persistence.Interceptors;
using CommunityBot.Infrastructure.Persistence.Items;
using CommunityBot.Infrastructure.Persistence.Members;
using CommunityBot.Infrastructure.Persistence.Rewards;
using CommunityBot.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityBot.Host;

public static partial class ServicesConfiguration
{
    /// <summary>
    /// Configures PostgreSQL, Entity Framework Core
    /// and persistence-related services.
    /// </summary>
    private static void ConfigurePersistence(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") 
                               ?? throw new InvalidOperationException(
                                   "Connection string 'Default' is missing in configuration.");

        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<AppDbContext>((provider, options) =>
        {
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();

            options.AddInterceptors(
                provider.GetRequiredService<AuditInterceptor>());
        });
        
        services.AddScoped<IPersistenceContext>(
            provider => provider.GetRequiredService<AppDbContext>());
        
        // Seeders
        services.AddScoped<ShopCatalogSeeder>();

        // Repositories
        
        // Members
        services.AddScoped<IMemberRepository, MemberRepository>();

        // Economy
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        
        // Items
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IShopItemRepository, ShopItemRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        
        // Rewards
        services.AddScoped<IRewardEntitlementRepository, RewardEntitlementRepository>();
        
        // Activities
        services.AddScoped<IActivityEventRepository, ActivityEventRepository>();
        services.AddScoped<IActivityCaptureGateRepository, ActivityCaptureGateRepository>();
        services.AddScoped<IActivitySubscriptionRepository, ActivitySubscriptionRepository>();
        services.AddScoped<IActivityConsumptionRepository, ActivityConsumptionRepository>();
        services.AddScoped<IActivityCaptureIncidentRepository, ActivityCaptureIncidentRepository>();
        services.AddScoped<IActivityReconciliationRepository, ActivityReconciliationRepository>();
        
        // Goals
        services.AddScoped<ICommunityGoalRepository, CommunityGoalRepository>();
    }
}