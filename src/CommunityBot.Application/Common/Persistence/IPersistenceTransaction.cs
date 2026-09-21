namespace CommunityBot.Application.Common.Persistence;

/// <summary>
/// Represents an explicit persistence transaction whose completion and lifetime are controlled by the caller.
/// </summary>
public interface IPersistenceTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);

    Task RollbackAsync(CancellationToken ct = default);
}