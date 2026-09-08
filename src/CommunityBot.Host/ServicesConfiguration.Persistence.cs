using CommunityBot.Application.Common.Persistence;
using CommunityBot.Application.Economy;
using CommunityBot.Application.Members;
using CommunityBot.Infrastructure.Persistence;
using CommunityBot.Infrastructure.Persistence.Economy;
using CommunityBot.Infrastructure.Persistence.Interceptors;
using CommunityBot.Infrastructure.Persistence.Members;
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

        // Repositories
        
        // Members
        services.AddScoped<IMemberRepository, MemberRepository>();

        // Economy
        services.AddScoped<ITransactionRepository, TransactionRepository>();
    }
}