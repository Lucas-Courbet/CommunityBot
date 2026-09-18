using CommunityBot.Application.Activities.Interfaces;
using CommunityBot.Application.Common.Persistence;
using CommunityBot.Core.Activities;
using CommunityBot.Core.Goals;

namespace CommunityBot.Application.Goals;

public sealed class CommunityGoalService(
    IPersistenceContext persistenceContext,
    ICommunityGoalRepository goalRepository,
    IActivitySubscriptionPlanService subscriptionPlanService)
    : ICommunityGoalService
{
    public async Task<CommunityGoal> CreateAsync(
        CommunityGoalCreateRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var goal = CommunityGoal.Create(
            request.Id,
            request.Title,
            request.TargetCount,
            request.StartsAt,
            request.EndsAt);

        await using var transaction = await persistenceContext.BeginTransactionAsync(ct);

        try
        {
            goalRepository.Add(goal);
            await persistenceContext.SaveChangesAsync(ct);

            await subscriptionPlanService.ArmAsync(
                ActivityConsumerType.CommunityGoal,
                goal.Id,
                [ActivityEventType.ShopPurchaseCompleted],
                goal.StartsAt,
                goal.EndsAt,
                ct);

            await transaction.CommitAsync(ct);

            return goal;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}