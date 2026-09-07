using CommunityBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace CommunityBot.Host;

/// <summary>
/// Execution entry point for the CommunityBot application.
/// </summary>
/// <remarks>
/// Handles application bootstrapping, logging configuration,
/// dependency registration and database schema initialization.
/// Discord gateway initialization is added separately by the
/// presentation layer.
/// </remarks>
internal abstract class Program
{
    /// <summary>
    /// Configures and runs the application host.
    /// </summary>
    /// <param name="args">Command-line arguments supplied at startup.</param>
    /// <returns>
    /// An exit code of 0 after a graceful shutdown,
    /// or 1 when startup terminates unexpectedly.
    /// </returns>
    public static async Task<int> Main(string[] args)
    {
        // Provides logging before the full dependency injection
        // container and configuration pipeline are available.
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override(
                "Microsoft",
                LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting CommunityBot host...");

            var builder =
                Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

            ConfigureConfiguration(builder);
            ConfigureLogger(builder);

            var host = builder.Build();

            await InitializeDatabaseAsync(host);

            await host.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(
                ex,
                "Host terminated unexpectedly");

            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    /// <summary>
    /// Applies all pending Entity Framework Core migrations
    /// before the application begins processing work.
    /// </summary>
    private static async Task InitializeDatabaseAsync(IHost host)
    {
        using var scope = host.Services.CreateScope();

        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation(
                "Applying pending database migrations...");

            var context =
                services.GetRequiredService<AppDbContext>();

            await context.Database.MigrateAsync();

            logger.LogInformation(
                "Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                ex,
                "An error occurred while initializing the database.");

            throw;
        }
    }

    /// <summary>
    /// Configures application configuration providers
    /// and dependency registration.
    /// </summary>
    /// <remarks>
    /// Precedence:
    /// appsettings.json →
    /// appsettings.{Environment}.json →
    /// environment variables.
    /// </remarks>
    private static void ConfigureConfiguration(
        HostApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile(
                "appsettings.json",
                optional: false,
                reloadOnChange: true)
            .AddJsonFile(
                $"appsettings.{builder.Environment.EnvironmentName}.json",
                optional: true)
            .AddEnvironmentVariables();

        ServicesConfiguration.Configure(
            builder.Services,
            builder.Configuration);
    }

    /// <summary>
    /// Configures Serilog as the application's logging provider.
    /// </summary>
    private static void ConfigureLogger(
        HostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        builder.Services.AddSerilog((services, configuration) =>
            configuration
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext());
    }
}