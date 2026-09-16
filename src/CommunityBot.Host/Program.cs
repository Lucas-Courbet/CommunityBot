using CommunityBot.Discord.Interactions;
using CommunityBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCord.Hosting.Services;
using Serilog;
using Serilog.Events;

namespace CommunityBot.Host;

/// <summary>
/// Execution entry point for the CommunityBot application.
/// </summary>
/// <remarks>
/// Handles application bootstrapping, logging configuration,
/// database schema initialization and Discord gateway startup.
/// </remarks>
internal abstract class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting CommunityBot host...");

            var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(
                new HostApplicationBuilderSettings
                {
                    Args = args,
                    ContentRootPath = AppContext.BaseDirectory
                });

            ConfigureConfiguration(builder);
            ConfigureLogger(builder);

            var host = builder.Build();

            await InitializeDatabaseAsync(host);

            if (builder.Configuration.GetValue<bool>("Discord:Enabled"))
                host.AddModules(typeof(ABaseSlashModule).Assembly);

            await host.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static async Task InitializeDatabaseAsync(IHost host)
    {
        using var scope = host.Services.CreateScope();

        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Applying pending database migrations...");

            var context = services.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();

            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    private static void ConfigureConfiguration(HostApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

        ServicesConfiguration.Configure(builder.Services, builder.Configuration);
    }

    private static void ConfigureLogger(HostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        builder.Services.AddSerilog((services, configuration) => configuration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());
    }
}