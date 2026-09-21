using CommunityBot.Application.Common.Persistence;
using CommunityBot.Application.Roles;
using CommunityBot.Core.Rewards;
using Microsoft.Extensions.Logging;

namespace CommunityBot.Application.Rewards;

/// <summary>
/// Delivers Role reward entitlements through an external role service.
/// </summary>
/// <remarks>
/// The external operation is intentionally executed outside the database transaction. Its outcome is
/// persisted afterwards in a short transaction that reloads and locks the entitlement, then verifies
/// that it still matches the snapshot used for the external operation before applying the transition.
/// </remarks>
public sealed class RoleRewardDeliveryHandler(
    IPersistenceContext persistenceContext,
    IRewardEntitlementRepository rewardEntitlementRepository,
    IRoleService roleService,
    ILogger<RoleRewardDeliveryHandler> logger)
    : IRewardDeliveryHandler
{
    public RewardType RewardType => RewardType.Role;

    /// <inheritdoc />
    public async Task<RewardDeliveryResult> DeliverAsync(long entitlementId, CancellationToken ct = default)
    {
        var snapshot = await rewardEntitlementRepository.GetDeliverySnapshotAsync(
            entitlementId, ct);

        if (snapshot is null)
            return RewardDeliveryResult.NotFound(entitlementId);

        if (snapshot.Status == RewardEntitlementStatus.Delivered)
            return RewardDeliveryResult.AlreadyFinalized(entitlementId);

        EnsureCompatibleSnapshot(snapshot);

        var roleResult = await roleService.AssignConfiguredRoleByKeyAsync(
            snapshot.MemberId,
            snapshot.RewardReference!,
            ct);

        return await FinalizeDeliveryAsync(snapshot, roleResult, ct);
    }

    private async Task<RewardDeliveryResult> FinalizeDeliveryAsync(
        RewardDeliverySnapshot snapshot,
        RoleOperationResult roleResult,
        CancellationToken ct)
    {
        RewardDeliveryResult result;

        await using var transaction = await persistenceContext.BeginTransactionAsync(ct);

        try
        {
            result = await ProcessFinalizationAsync(snapshot, roleResult, ct);
            await persistenceContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(CancellationToken.None);

            logger.LogError(
                ex,
                "Failed to finalize Role reward entitlement {EntitlementId}.",
                snapshot.EntitlementId);

            throw;
        }

        return result;
    }

    private async Task<RewardDeliveryResult> ProcessFinalizationAsync(
        RewardDeliverySnapshot snapshot,
        RoleOperationResult roleResult,
        CancellationToken ct)
    {
        var entitlement = await rewardEntitlementRepository.GetByIdForUpdateAsync(
            snapshot.EntitlementId, ct);

        if (entitlement is null)
        {
            throw new InvalidOperationException(
                $"Reward entitlement {snapshot.EntitlementId} disappeared during Role delivery.");
        }

        if (entitlement.IsFinalized)
            return RewardDeliveryResult.AlreadyFinalized(entitlement.Id);

        EnsureSnapshotStillMatches(entitlement, snapshot);

        return ApplyRoleResult(entitlement, roleResult);
    }

    private static RewardDeliveryResult ApplyRoleResult(
        RewardEntitlement entitlement,
        RoleOperationResult roleResult)
    {
        switch (roleResult.Status)
        {
            case RoleOperationStatus.Assigned:
            case RoleOperationStatus.AlreadyAssigned:
                entitlement.MarkAsDelivered();
                return RewardDeliveryResult.Delivered(entitlement.Id);

            case RoleOperationStatus.NotConfigured:
            case RoleOperationStatus.Forbidden:
            case RoleOperationStatus.NotFound:
            case RoleOperationStatus.Failure:
                entitlement.RecordDeliveryFailure(
                    $"Role delivery failed with status {roleResult.Status}: {roleResult.Message}");

                return RewardDeliveryResult.Failed(
                    entitlement.Id,
                    roleResult.Message ?? "Role delivery failed.");

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(roleResult),
                    roleResult.Status,
                    "Unsupported role operation status.");
        }
    }

    private static void EnsureCompatibleSnapshot(RewardDeliverySnapshot snapshot)
    {
        if (snapshot.RewardType != RewardType.Role)
        {
            throw new InvalidOperationException(
                $"Reward entitlement {snapshot.EntitlementId} has type {snapshot.RewardType} " +
                "but was routed to the Role handler.");
        }

        if (string.IsNullOrWhiteSpace(snapshot.RewardReference))
        {
            throw new InvalidOperationException(
                $"Role reward entitlement {snapshot.EntitlementId} must define a role key.");
        }

        if (snapshot.Quantity != 1)
        {
            throw new InvalidOperationException(
                $"Role reward entitlement {snapshot.EntitlementId} must have quantity 1.");
        }
    }

    private static void EnsureSnapshotStillMatches(
        RewardEntitlement entitlement,
        RewardDeliverySnapshot snapshot)
    {
        if (entitlement.MemberId != snapshot.MemberId ||
            entitlement.RewardType != snapshot.RewardType ||
            entitlement.Quantity != snapshot.Quantity ||
            !string.Equals(
                entitlement.RewardReference,
                snapshot.RewardReference,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Reward entitlement {entitlement.Id} changed during Role delivery.");
        }
    }
}