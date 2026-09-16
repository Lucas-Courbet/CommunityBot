using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities.Interfaces;

public interface IActivityCaptureGateRepository
{
    Task<ActivityCaptureGate?> GetForUpdateAsync(
        ActivityEventType eventType,
        CancellationToken ct = default);
}