using CommunityBot.Application.Activities;
using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Core.Activities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CommunityBot.Infrastructure.Activities;

public sealed class ActivityCaptureService(
    ActivityCaptureStore store,
    ActivityCaptureTransactionCoordinator transactionCoordinator,
    ILogger<ActivityCaptureService> logger)
    : IActivityCaptureService
{
    private const int ContractVersion = 1;
    private const int SourceReferenceMaxLength = 150;

    private const string NominalSavepoint = "activity_capture_nominal";
    private const string ConservativeSavepoint = "activity_capture_conservative";
    private const string IncidentSavepoint = "activity_capture_incident";

    public async Task<ActivityCaptureResult> CaptureAsync(
        ActivityEventCandidate candidate,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        ValidateCandidate(candidate);
        transactionCoordinator.EnsureReadyForCapture();

        try
        {
            return await CaptureNominallyAsync(candidate, ct);
        }
        catch (Exception nominalException)
            when (CanContinueAfterIsolatedFailure(nominalException))
        {
            return await HandleNominalFailureAsync(candidate, nominalException, ct);
        }
    }

    private async Task<ActivityCaptureResult> CaptureNominallyAsync(
        ActivityEventCandidate candidate,
        CancellationToken ct)
    {
        CaptureRegistration? registration = null;

        return await transactionCoordinator.ExecuteInSavepointAsync(
            NominalSavepoint,
            async () =>
            {
                registration = await PrepareCaptureAsync(candidate, ct);

                if (registration is null)
                    return new ActivityCaptureResult(ActivityCaptureStatus.NotCaptured, null);

                await store.PersistCaptureAsync(
                    registration.ActivityEvent,
                    registration.Consumptions,
                    ct);

                return new ActivityCaptureResult(
                    ActivityCaptureStatus.Captured,
                    registration.ActivityEvent.EventId);
            },
            () =>
            {
                if (registration is not null)
                {
                    store.DiscardCapture(
                        registration.ActivityEvent,
                        registration.Consumptions);
                }
            },
            ct);
    }

    private async Task<ActivityCaptureResult> HandleNominalFailureAsync(
        ActivityEventCandidate candidate,
        Exception nominalException,
        CancellationToken ct)
    {
        logger.LogWarning(
            nominalException,
            "Nominal activity capture failed for {EventType} and member {MemberId}. " +
            "Attempting conservative capture.",
            candidate.EventType,
            candidate.MemberId);

        try
        {
            return await CaptureConservativelyAsync(candidate, ct);
        }
        catch (Exception conservativeException)
            when (CanContinueAfterIsolatedFailure(conservativeException))
        {
            return await HandleCaptureLossAsync(
                candidate,
                nominalException,
                conservativeException,
                ct);
        }
    }

    private async Task<ActivityCaptureResult> CaptureConservativelyAsync(
        ActivityEventCandidate candidate,
        CancellationToken ct)
    {
        ActivityEvent? pendingEvent = null;
        ActivityReconciliation? pendingReconciliation = null;

        var persistedEvent = await transactionCoordinator.ExecuteInSavepointAsync(
            ConservativeSavepoint,
            async () =>
            {
                pendingEvent = CreateActivityEvent(candidate);
                pendingReconciliation = new ActivityReconciliation(
                    pendingEvent,
                    DateTime.UtcNow);

                await store.PersistConservativeCaptureAsync(
                    pendingEvent,
                    pendingReconciliation,
                    ct);

                return pendingEvent;
            },
            () =>
            {
                if (pendingEvent is not null && pendingReconciliation is not null)
                {
                    store.DiscardConservativeCapture(
                        pendingEvent,
                        pendingReconciliation);
                }
            },
            ct);

        logger.LogWarning(
            "Activity {EventType} for member {MemberId} was captured conservatively " +
            "as event {EventId}. Reconciliation is required.",
            candidate.EventType,
            candidate.MemberId,
            persistedEvent.EventId);

        return new ActivityCaptureResult(
            ActivityCaptureStatus.CapturedConservatively,
            persistedEvent.EventId);
    }

    private async Task<ActivityCaptureResult> HandleCaptureLossAsync(
        ActivityEventCandidate candidate,
        Exception nominalException,
        Exception conservativeException,
        CancellationToken ct)
    {
        logger.LogError(
            conservativeException,
            "Conservative activity capture also failed for {EventType} and member {MemberId}.",
            candidate.EventType,
            candidate.MemberId);

        try
        {
            await PersistCaptureIncidentAsync(
                candidate,
                nominalException,
                conservativeException,
                ct);
        }
        catch (Exception incidentException)
            when (CanContinueAfterIsolatedFailure(incidentException))
        {
            logger.LogCritical(
                incidentException,
                "Activity capture loss could not be persisted for {EventType} and member {MemberId}.",
                candidate.EventType,
                candidate.MemberId);
        }

        return new ActivityCaptureResult(
            ActivityCaptureStatus.NotCapturedDueToFailure,
            null);
    }

    private async Task PersistCaptureIncidentAsync(
        ActivityEventCandidate candidate,
        Exception nominalException,
        Exception conservativeException,
        CancellationToken ct)
    {
        ActivityCaptureIncident? pendingIncident = null;

        await transactionCoordinator.ExecuteInSavepointAsync(
            IncidentSavepoint,
            async () =>
            {
                pendingIncident = new ActivityCaptureIncident
                {
                    EventType = candidate.EventType,
                    MemberId = candidate.MemberId,
                    OccurredAt = candidate.OccurredAt,
                    OccurrenceCount = candidate.OccurrenceCount,
                    SourceReference = candidate.SourceReference,
                    DetectedAt = DateTime.UtcNow,
                    NominalError = DescribeFailure(nominalException),
                    ConservativeError = DescribeFailure(conservativeException)
                };

                await store.PersistIncidentAsync(pendingIncident, ct);

                return pendingIncident;
            },
            () =>
            {
                if (pendingIncident is not null)
                    store.DiscardIncident(pendingIncident);
            },
            ct);
    }

    private async Task<CaptureRegistration?> PrepareCaptureAsync(
        ActivityEventCandidate candidate,
        CancellationToken ct)
    {
        var subscriptions = await store.ResolveMatchingSubscriptionsAsync(
            candidate.EventType,
            candidate.OccurredAt,
            ct);

        if (subscriptions.Count == 0)
            return null;

        var activityEvent = CreateActivityEvent(candidate);

        var consumptions = subscriptions
            .Select(subscription => new ActivityConsumption(activityEvent, subscription))
            .ToList();

        return new CaptureRegistration(activityEvent, consumptions);
    }

    private static ActivityEvent CreateActivityEvent(ActivityEventCandidate candidate)
        => new()
        {
            EventId = Guid.NewGuid(),
            EventType = candidate.EventType,
            MemberId = candidate.MemberId,
            OccurredAt = candidate.OccurredAt,
            OccurrenceCount = candidate.OccurrenceCount,
            CapturedAt = DateTime.UtcNow,
            ContractVersion = ContractVersion,
            SourceReference = candidate.SourceReference
        };

    private static void ValidateCandidate(ActivityEventCandidate candidate)
    {
        if (!Enum.IsDefined(candidate.EventType))
            throw new ArgumentOutOfRangeException(nameof(candidate), candidate.EventType, "Unsupported activity event type.");

        if (candidate.OccurredAt.Kind != DateTimeKind.Utc)
            throw new ArgumentException("OccurredAt must use UTC.", nameof(candidate));

        if (candidate.OccurrenceCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(candidate), "Occurrence count must be strictly positive.");

        if (candidate.SourceReference is null)
            return;

        if (string.IsNullOrWhiteSpace(candidate.SourceReference))
            throw new ArgumentException("Source reference cannot be blank.", nameof(candidate));

        if (candidate.SourceReference.Length > SourceReferenceMaxLength)
            throw new ArgumentException(
                $"Source reference cannot exceed {SourceReferenceMaxLength} characters.",
                nameof(candidate));
    }

    private static bool CanContinueAfterIsolatedFailure(Exception exception)
    {
        if (exception is ActivityCaptureTransactionCoordinator.CaptureTransactionRecoveryException
            or OperationCanceledException)
        {
            return false;
        }

        var postgresException = FindPostgresException(exception);

        if (postgresException?.SqlState is
            PostgresErrorCodes.SerializationFailure or
            PostgresErrorCodes.DeadlockDetected)
        {
            return false;
        }

        return exception is
            MissingActivityCaptureGateException or
            DbUpdateException or
            NpgsqlException or
            TimeoutException;
    }

    private static string DescribeFailure(Exception exception)
    {
        var postgresException = FindPostgresException(exception);

        return postgresException is null
            ? $"{exception.GetType().Name}: {exception.Message}"
            : $"{exception.GetType().Name}: PostgreSQL {postgresException.SqlState} - {postgresException.MessageText}";
    }

    private static PostgresException? FindPostgresException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgresException)
                return postgresException;
        }

        return null;
    }

    private sealed record CaptureRegistration(
        ActivityEvent ActivityEvent,
        IReadOnlyList<ActivityConsumption> Consumptions);
}