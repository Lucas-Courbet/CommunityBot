using CommunityBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace CommunityBot.Infrastructure.Activities;

/// <summary>
/// Coordinates savepoint isolation for activity capture inside a caller-owned transaction.
/// </summary>
/// <remarks>
/// This coordinator never completes the outer transaction. After an isolated capture failure,
/// it restores both the database savepoint and the corresponding EF Core tracking state before
/// allowing the capture workflow to degrade safely.
/// </remarks>
public sealed class ActivityCaptureTransactionCoordinator(AppDbContext context)
{
    /// <summary>
    /// Verifies that capture can safely begin inside the caller-owned transaction.
    /// </summary>
    /// <remarks>
    /// Source-owned changes must already have been persisted inside the transaction so that a later
    /// capture <c>SaveChanges</c> cannot accidentally include them in a capture savepoint.
    /// </remarks>
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

    /// <summary>
    /// Executes one capture persistence stage inside a dedicated savepoint.
    /// </summary>
    /// <remarks>
    /// If the operation fails, the database is rolled back to the savepoint and
    /// <paramref name="rollbackCleanup"/> restores the corresponding EF tracking state.
    /// Failure to restore either side is considered unrecoverable for safe capture degradation.
    /// </remarks>
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

    /// <summary>
    /// Indicates that activity capture could not safely restore the caller-owned transaction after an isolated failure.
    /// </summary>
    internal sealed class CaptureTransactionRecoveryException(
        string message,
        Exception innerException)
        : InvalidOperationException(message, innerException);
}