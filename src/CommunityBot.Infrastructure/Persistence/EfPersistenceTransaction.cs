using CommunityBot.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace CommunityBot.Infrastructure.Persistence;

internal sealed class EfPersistenceTransaction(
    IDbContextTransaction transaction)
    : IPersistenceTransaction
{
    public Task CommitAsync(CancellationToken ct = default)
        => transaction.CommitAsync(ct);

    public Task RollbackAsync(CancellationToken ct = default)
        => transaction.RollbackAsync(ct);

    public ValueTask DisposeAsync()
        => transaction.DisposeAsync();
}