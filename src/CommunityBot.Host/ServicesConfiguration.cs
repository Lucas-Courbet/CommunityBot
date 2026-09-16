using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityBot.Host;

/// <summary>
/// Centralizes application composition and dependency registration.
/// </summary>
public static partial class ServicesConfiguration
{
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration)
    {
        ConfigurePersistence(services, configuration);
        ConfigureApplicationServices(services);
        ConfigureActivities(services, configuration);
        ConfigureDiscord(services, configuration);
    }
}