using CommunityBot.Core.Common;

namespace CommunityBot.Application.Common.Persistence;

/// <summary>
/// Defines common persistence operations shared by application repositories.
/// </summary>
public interface IBaseRepository<TEntity, in TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Retrieves an entity by its primary key.
    /// </summary>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all persisted entities of this type.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Determines whether an entity exists.
    /// </summary>
    Task<bool> ExistsAsync(TKey id, CancellationToken ct = default);

    /// <summary>
    /// Marks an entity for insertion.
    /// </summary>
    void Add(TEntity entity);

    /// <summary>
    /// Marks multiple entities for insertion.
    /// </summary>
    void AddRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Marks an entity for update.
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Marks multiple entities for update.
    /// </summary>
    void UpdateRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Marks an entity for removal.
    /// </summary>
    void Remove(TEntity entity);

    /// <summary>
    /// Marks multiple entities for removal.
    /// </summary>
    void RemoveRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Persists all tracked changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}