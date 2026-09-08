using CommunityBot.Discord.Interactions;
using CommunityBot.Discord.Shop;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityBot.Discord;

public static class DiscordPresentationConfiguration
{
    public static IServiceCollection AddDiscordPresentation(this IServiceCollection services)
    {
        services.AddScoped<IResponseService, ResponseService>();
        services.AddScoped<IShopRenderService, ShopRenderService>();

        return services;
    }
}