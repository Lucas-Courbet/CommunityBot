using CommunityBot.Infrastructure.Persistence;
using CommunityBot.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CommunityBot.Tests.Integration;

/// <summary>
/// Provides a shared disposable PostgreSQL instance for integration tests.
/// </summary>
public sealed class PostgreSqlIntegrationFixture : IAsyncLifetime
{
    private const string DatabaseImage = "postgres:17-alpine";

    private readonly PostgreSqlContainer _dbContainer =
        new PostgreSqlBuilder(DatabaseImage)
            .WithDatabase("communitybot_tests")
            .Build();

    private DbContextOptions<AppDbContext> _dbContextOptions = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        _dbContextOptions =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_dbContainer.GetConnectionString())
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(new AuditInterceptor())
                .Options;

        await using var context = CreateDbContext();

        await context.Database.MigrateAsync();
    }

    public AppDbContext CreateDbContext()
        => new(_dbContextOptions);

    /// <summary>
    /// Removes persisted test data while preserving the migrated schema
    /// and Entity Framework migration history.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await using var context = CreateDbContext();

        await context.Database.ExecuteSqlRawAsync(
            """
            DO $$
            DECLARE
                table_record record;
            BEGIN
                FOR table_record IN
                    SELECT tablename
                    FROM pg_tables
                    WHERE schemaname = 'public'
                      AND tablename <> '__EFMigrationsHistory'
                LOOP
                    EXECUTE format(
                        'TRUNCATE TABLE %I RESTART IDENTITY CASCADE;',
                        table_record.tablename);
                END LOOP;
            END $$;
            """);
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}