using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Infrastructure.Activities;
using CommunityBot.Infrastructure.Activities.Consumption;
using CommunityBot.Infrastructure.Activities.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityBot.Host;

public static partial class ServicesConfiguration
{
    private static void ConfigureActivities(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        services.Configure<ActivityConsumptionRetryOptions>(
            configuration.GetSection(ActivityConsumptionRetryOptions.SectionName));

        services.Configure<ActivityConsumptionWorkerOptions>(
            configuration.GetSection(ActivityConsumptionWorkerOptions.SectionName));

        services.Configure<ActivityReconciliationWorkerOptions>(
            configuration.GetSection(ActivityReconciliationWorkerOptions.SectionName));

        services.AddScoped<ActivityCaptureStore>();
        services.AddScoped<ActivityCaptureTransactionCoordinator>();

        services.AddScoped<IActivityCaptureService, ActivityCaptureService>();
        services.AddScoped<IActivitySubscriptionPlanService, ActivitySubscriptionPlanService>();
        services.AddScoped<IActivityConsumptionService, ActivityConsumptionService>();
        services.AddScoped<IActivityConsumptionFailureService, ActivityConsumptionFailureService>();
        services.AddScoped<IActivityReconciliationService, ActivityReconciliationService>();

        services.AddHostedService<ActivityConsumptionWorker>();
        services.AddHostedService<ActivityReconciliationWorker>();
    }
}