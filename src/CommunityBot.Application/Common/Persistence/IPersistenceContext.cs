namespace CommunityBot.Application.Common.Persistence;

/// <summary>
/// Exposes persistence operations required by atomic application workflows.
/// </summary>
public interface IPersistenceContext
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task<IPersistenceTransaction> BeginTransactionAsync(CancellationToken ct = default);
}