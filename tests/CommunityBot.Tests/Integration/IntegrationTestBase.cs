using CommunityBot.Infrastructure.Persistence;

namespace CommunityBot.Tests.Integration;

public abstract class IntegrationTestBase(
    PostgreSqlIntegrationFixture fixture)
    : IAsyncLifetime
{
    protected AppDbContext Context { get; set; } = null!;

    public virtual async Task InitializeAsync()
    {
        await fixture.ResetDatabaseAsync();

        Context = fixture.CreateDbContext();
    }

    protected AppDbContext CreateDbContext()
        => fixture.CreateDbContext();

    public virtual async Task DisposeAsync()
    {
        await Context.DisposeAsync();
    }
}
