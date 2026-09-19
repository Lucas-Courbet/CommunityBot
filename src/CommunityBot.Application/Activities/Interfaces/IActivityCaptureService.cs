namespace CommunityBot.Application.Activities.Interfaces;

/// <summary>
/// Captures durable activity facts inside a caller-owned database transaction.
/// </summary>
public interface IActivityCaptureService
{
    Task<ActivityCaptureResult> CaptureAsync(ActivityEventCandidate candidate, CancellationToken ct = default);
}