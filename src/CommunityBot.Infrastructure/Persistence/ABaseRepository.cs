using CommunityBot.Application.Common.Persistence;
using CommunityBot.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace CommunityBot.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core implementation of the common repository operations.
/// </summary>
public abstract class ABaseRepository<TEntity, TKey>
    : IBaseRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : notnull
{
    protected AppDbContext Context { get; }

    /// <summary>
    /// Entity Framework set associated with the repository entity type.
    /// </summary>
    protected DbSet<TEntity> DbSet { get; }

    protected ABaseRepository(AppDbContext context)
    {
        Context = context
            ?? throw new ArgumentNullException(nameof(context));

        DbSet = context.Set<TEntity>();
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(
        TKey id,
        CancellationToken ct = default)
        => await DbSet.FindAsync([id], ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> GetAllAsync(
        CancellationToken ct = default)
        => await DbSet
            .AsNoTracking()
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        TKey id,
        CancellationToken ct = default)
        => await DbSet.AnyAsync(
            entity => entity.Id.Equals(id),
            ct);

    /// <inheritdoc />
    public void Add(TEntity entity)
        => DbSet.Add(entity);

    /// <inheritdoc />
    public void AddRange(IEnumerable<TEntity> entities)
        => DbSet.AddRange(entities);

    /// <inheritdoc />
    public void Update(TEntity entity)
    {
        var entry = Context.Entry(entity);

        if (entry.State == EntityState.Detached)
        {
            var key = entry.Metadata.FindPrimaryKey();

            var keyValues = key!.Properties
                .Select(property => entry.Property(property.Name).CurrentValue)
                .ToArray();

            var existingEntity = DbSet.Local.FirstOrDefault(candidate =>
            {
                var candidateEntry = Context.Entry(candidate);

                var candidateKeyValues = candidateEntry.Metadata
                    .FindPrimaryKey()!
                    .Properties
                    .Select(property =>
                        candidateEntry.Property(property.Name).CurrentValue)
                    .ToArray();

                return candidateKeyValues.SequenceEqual(keyValues);
            });

            if (existingEntity is not null)
            {
                Context.Entry(existingEntity)
                    .CurrentValues
                    .SetValues(entity);

                return;
            }
        }

        DbSet.Update(entity);
    }

    /// <inheritdoc />
    public void UpdateRange(IEnumerable<TEntity> entities)
        => DbSet.UpdateRange(entities);

    /// <inheritdoc />
    public void Remove(TEntity entity)
        => DbSet.Remove(entity);

    /// <inheritdoc />
    public void RemoveRange(IEnumerable<TEntity> entities)
        => DbSet.RemoveRange(entities);

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(
        CancellationToken ct = default)
        => Context.SaveChangesAsync(ct);
}