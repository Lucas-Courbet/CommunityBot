namespace CommunityBot.Application.Common.Persistence;

/// <summary>
/// Exposes persistence operations required by atomic application workflows.
/// </summary>
public interface IPersistenceContext
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Begins an explicit transaction whose completion and lifetime are controlled by the caller.
    /// </summary>
    Task<IPersistenceTransaction> BeginTransactionAsync(CancellationToken ct = default);
}