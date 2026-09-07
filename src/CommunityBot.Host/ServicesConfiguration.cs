using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityBot.Host;

/// <summary>
/// Centralizes application composition and dependency registration.
/// </summary>
public static partial class ServicesConfiguration
{
    /// <summary>
    /// Registers the services required by the application.
    /// </summary>
    public static void Configure(
        IServiceCollection services,
        IConfiguration configuration)
    {
        ConfigurePersistence(services, configuration);
        ConfigureApplicationServices(services);
    }
}