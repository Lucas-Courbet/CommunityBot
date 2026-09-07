using Microsoft.EntityFrameworkCore;

namespace CommunityBot.Tests.Integration;

[Collection(IntegrationTestCollection.Name)]
public sealed class MigrationTests(
    PostgreSqlIntegrationFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Database_ShouldHaveInitialMigrationApplied()
    {
        var appliedMigrations = await Context.Database
            .GetAppliedMigrationsAsync();

        Assert.Contains(appliedMigrations, 
            migration => migration.EndsWith(
                "_InitialCreate",
                StringComparison.Ordinal));
    }
}