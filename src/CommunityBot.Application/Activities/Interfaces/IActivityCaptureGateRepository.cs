using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities.Interfaces;

/// <summary>
/// Provides persistence operations required to coordinate capture for an activity type.
/// </summary>
public interface IActivityCaptureGateRepository
{
    /// <summary>
    /// Retrieves the permanent capture gate and acquires a row lock for the current transaction.
    /// </summary>
    Task<ActivityCaptureGate?> GetForUpdateAsync(ActivityEventType eventType, CancellationToken ct = default);
}