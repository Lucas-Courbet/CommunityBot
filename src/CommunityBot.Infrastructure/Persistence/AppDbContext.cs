using CommunityBot.Application.Common.Persistence;
using CommunityBot.Core.Economy;
using CommunityBot.Core.Members;
using Microsoft.EntityFrameworkCore;

namespace CommunityBot.Infrastructure.Persistence;

/// <summary>
/// Primary Entity Framework Core database context for the application.
/// </summary>
public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options)
    : DbContext(options), IPersistenceContext
{
    public DbSet<Member> Members => Set<Member>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<ulong>().HaveConversion<long>();
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <inheritdoc />
    public async Task<IPersistenceTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => new EfPersistenceTransaction(await Database.BeginTransactionAsync(ct));
}