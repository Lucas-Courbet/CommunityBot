using CommunityBot.Core.Activities;

namespace CommunityBot.Application.Activities;

public interface IActivityCaptureGateRepository
{
    Task<ActivityCaptureGate?> GetForUpdateAsync(
        ActivityEventType eventType,
        CancellationToken ct = default);
}