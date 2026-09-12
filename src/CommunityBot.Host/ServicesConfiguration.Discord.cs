using CommunityBot.Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;

namespace CommunityBot.Host;

public static partial class ServicesConfiguration
{
    private static void ConfigureDiscord(IServiceCollection services, IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("Discord:Enabled"))
            return;

        var token = configuration["Discord:Token"];

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException(
                "Discord is enabled but no bot token is configured.");

        services
            .AddDiscordGateway(options =>
            {
                options.Token = token;
                options.Intents = GatewayIntents.Guilds;
            })
            .AddApplicationCommands()
            .AddComponentInteractions()
            .AddDiscordPresentation();
    }
}