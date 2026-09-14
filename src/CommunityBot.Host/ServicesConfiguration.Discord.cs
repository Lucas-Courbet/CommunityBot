using CommunityBot.Application.Rewards;
using CommunityBot.Application.Roles;
using CommunityBot.Discord;
using CommunityBot.Discord.Roles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;

namespace CommunityBot.Host;

public static partial class ServicesConfiguration
{
    private static void ConfigureDiscord(
        IServiceCollection services,
        IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("Discord:Enabled"))
            return;

        var token = configuration["Discord:Token"];

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException(
                "Discord is enabled but no bot token is configured.");

        var guildId = configuration.GetValue<ulong>("Discord:GuildId");

        if (guildId == 0)
            throw new InvalidOperationException(
                "Discord is enabled but no guild ID is configured.");

        var roles = configuration
                        .GetSection("Discord:Roles")
                        .Get<Dictionary<string, ulong>>()
                    ?? new Dictionary<string, ulong>();

        services
            .AddDiscordGateway(options =>
            {
                options.Token = token;
                options.Intents = GatewayIntents.Guilds;
            })
            .AddApplicationCommands()
            .AddComponentInteractions()
            .AddDiscordPresentation();

        services.AddSingleton(new DiscordRoleSettings(guildId, roles));

        services.AddScoped<IRoleConfiguration, DiscordRoleConfiguration>();
        services.AddScoped<IRoleAdapter, DiscordRoleAdapter>();
        services.AddScoped<IRoleService, RoleService>();

        services.AddScoped<IRewardDeliveryHandler, RoleRewardDeliveryHandler>();
    }
}