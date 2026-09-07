using CommunityBot.Application.Members;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityBot.Host;

public static partial class ServicesConfiguration
{
    /// <summary>
    /// Registers application services and use-case orchestrators.
    /// </summary>
    private static void ConfigureApplicationServices(
        IServiceCollection services)
    {
        // Members
        services.AddScoped<IMemberService, MemberService>();
    }
}