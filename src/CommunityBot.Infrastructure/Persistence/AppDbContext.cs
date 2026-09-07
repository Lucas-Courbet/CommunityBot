using CommunityBot.Core.Members;
using Microsoft.EntityFrameworkCore;

namespace CommunityBot.Infrastructure.Persistence;

/// <summary>
/// Primary Entity Framework Core database context for the application.
/// </summary>
public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Member> Members => Set<Member>();

    /// <inheritdoc />
    protected override void ConfigureConventions(
        ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<ulong>()
            .HaveConversion<long>();
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}