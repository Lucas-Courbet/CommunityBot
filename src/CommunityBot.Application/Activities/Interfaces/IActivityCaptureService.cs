namespace CommunityBot.Application.Activities.Interfaces;

/// <summary>
/// Coordinates durable capture of source activity facts inside a caller-owned database transaction.
/// </summary>
/// <remarks>
/// The caller must own an active transaction and persist its own pending changes before capture starts.
/// Capture may use internal savepoints and persist activity-owned state, but never commits or rolls back
/// the caller-owned transaction. Recoverable nominal failures may fall back to conservative capture
/// for later reconciliation.
/// </remarks>
public interface IActivityCaptureService
{
    Task<ActivityCaptureResult> CaptureAsync(ActivityEventCandidate candidate, CancellationToken ct = default);
}