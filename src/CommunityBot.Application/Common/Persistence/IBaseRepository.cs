using CommunityBot.Core.Common;

namespace CommunityBot.Application.Common.Persistence;

/// <summary>
/// Defines common persistence operations shared by application repositories.
/// </summary>
public interface IBaseRepository<TEntity, in TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : notnull
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);

    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);

    Task<bool> ExistsAsync(TKey id, CancellationToken ct = default);

    void Add(TEntity entity);

    void AddRange(IEnumerable<TEntity> entities);

    void Update(TEntity entity);

    void UpdateRange(IEnumerable<TEntity> entities);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}