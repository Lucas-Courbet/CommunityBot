namespace CommunityBot.Application.Common.Persistence;

/// <summary>
/// Represents an explicit persistence transaction controlled by an application workflow.
/// </summary>
public interface IPersistenceTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);

    Task RollbackAsync(CancellationToken ct = default);
}