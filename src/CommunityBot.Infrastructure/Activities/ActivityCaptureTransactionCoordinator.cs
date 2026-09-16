using CommunityBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace CommunityBot.Infrastructure.Activities;

public sealed class ActivityCaptureTransactionCoordinator(AppDbContext context)
{
    public void EnsureReadyForCapture()
    {
        if (context.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Activity capture requires an active caller-owned transaction.");

        if (context.ChangeTracker.HasChanges())
        {
            throw new InvalidOperationException(
                "Pending source changes must be persisted inside the active transaction " +
                "before activity capture begins.");
        }
    }

    public async Task<TResult> ExecuteInSavepointAsync<TResult>(
        string savepoint,
        Func<Task<TResult>> operation,
        Action rollbackCleanup,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(savepoint);
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(rollbackCleanup);

        var transaction = GetCurrentTransaction();

        try
        {
            await transaction.CreateSavepointAsync(savepoint, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new CaptureTransactionRecoveryException(
                "Activity capture could not establish its transaction savepoint.",
                ex);
        }

        try
        {
            var result = await operation();
            await transaction.ReleaseSavepointAsync(savepoint, ct);
            return result;
        }
        catch (Exception ex)
        {
            await RecoverFailedStageAsync(
                transaction,
                savepoint,
                ex,
                rollbackCleanup);

            throw;
        }
    }

    private IDbContextTransaction GetCurrentTransaction()
        => context.Database.CurrentTransaction
           ?? throw new CaptureTransactionRecoveryException(
               "Activity capture lost access to the caller-owned transaction.",
               new InvalidOperationException("No active database transaction is available."));

    private static async Task RecoverFailedStageAsync(
        IDbContextTransaction transaction,
        string savepoint,
        Exception stageException,
        Action cleanup)
    {
        Exception? recoveryException = null;

        try
        {
            await transaction.RollbackToSavepointAsync(savepoint, CancellationToken.None);
            await transaction.ReleaseSavepointAsync(savepoint, CancellationToken.None);
        }
        catch (Exception ex)
        {
            recoveryException = ex;
        }

        try
        {
            cleanup();
        }
        catch (Exception ex)
        {
            recoveryException = recoveryException is null
                ? ex
                : new AggregateException(recoveryException, ex);
        }

        if (recoveryException is null)
            return;

        throw new CaptureTransactionRecoveryException(
            "Activity capture failed and the caller-owned transaction could not be safely restored.",
            new AggregateException(stageException, recoveryException));
    }

    internal sealed class CaptureTransactionRecoveryException(
        string message,
        Exception innerException)
        : InvalidOperationException(message, innerException);
}