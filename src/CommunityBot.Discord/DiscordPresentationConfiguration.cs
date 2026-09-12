using CommunityBot.Discord.Interactions;
using CommunityBot.Discord.Pagination;
using CommunityBot.Discord.Shop;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityBot.Discord;

public static class DiscordPresentationConfiguration
{
    public static IServiceCollection AddDiscordPresentation(this IServiceCollection services)
    {
        // Interactions
        services.AddScoped<IResponseService, ResponseService>();

        // Pagination
        services.AddSingleton<IPaginationService, PaginationService>();
        services.AddScoped<IPaginationDispatcher, PaginationDispatcher>();
        services.AddScoped<IPaginationStrategy, ShopPaginationStrategy>();

        // Shop
        services.AddScoped<IShopRenderService, ShopRenderService>();
        services.AddScoped<IShopPurchaseInteractionService, ShopPurchaseInteractionService>();

        return services;
    }
}