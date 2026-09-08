using CommunityBot.Application.Economy;
using CommunityBot.Application.Members;
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

        // Members
        services.AddScoped<IMemberService, MemberService>();
    }
}