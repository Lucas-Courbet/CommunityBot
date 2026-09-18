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
        ConfigureActivityOptions(services, configuration);

        services.AddSingleton(TimeProvider.System);

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

    private static void ConfigureActivityOptions(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ActivityConsumptionRetryOptions>()
            .Bind(configuration.GetSection(
                ActivityConsumptionRetryOptions.SectionName))
            .Validate(
                options => options.MaxAttempts > 0,
                "Activity consumption MaxAttempts must be strictly positive.")
            .Validate(
                options => options.RetryDelay > TimeSpan.Zero,
                "Activity consumption RetryDelay must be strictly positive.")
            .ValidateOnStart();

        services.AddOptions<ActivityConsumptionWorkerOptions>()
            .Bind(configuration.GetSection(
                ActivityConsumptionWorkerOptions.SectionName))
            .Validate(
                options => options.IdleDelay > TimeSpan.Zero,
                "Activity consumption IdleDelay must be strictly positive.")
            .Validate(
                options => options.FailureDelay > TimeSpan.Zero,
                "Activity consumption FailureDelay must be strictly positive.")
            .ValidateOnStart();

        services.AddOptions<ActivityReconciliationWorkerOptions>()
            .Bind(configuration.GetSection(
                ActivityReconciliationWorkerOptions.SectionName))
            .Validate(
                options => options.IdleDelay > TimeSpan.Zero,
                "Activity reconciliation IdleDelay must be strictly positive.")
            .Validate(
                options => options.FailureDelay > TimeSpan.Zero,
                "Activity reconciliation FailureDelay must be strictly positive.")
            .ValidateOnStart();
    }
}