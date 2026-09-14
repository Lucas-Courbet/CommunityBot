using CommunityBot.Application.Rewards;
using CommunityBot.Application.Roles;
using CommunityBot.Core.Members;
using CommunityBot.Core.Rewards;
using CommunityBot.Infrastructure.Persistence;
using CommunityBot.Infrastructure.Persistence.Rewards;
using CommunityBot.Tests.Integration.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CommunityBot.Tests.Integration.Rewards;

[Collection(IntegrationTestCollection.Name)]
public sealed class RoleRewardDeliveryHandlerIntegrationTests(
    PostgreSqlIntegrationFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string RoleKey = "community-member";

    [Fact]
    public async Task DeliverAsync_ShouldCallExternalRoleServiceOutsideDatabaseTransaction()
    {
        const ulong memberId = 5001UL;

        var entitlementId = await SeedRoleEntitlementAsync(memberId);

        var roleService = new TransactionObservingRoleService(Context);

        var handler = CreateHandler(Context, roleService);

        var result = await handler.DeliverAsync(entitlementId);

        await using var assertionContext = CreateDbContext();

        var entitlement = await assertionContext.RewardEntitlements
            .SingleAsync(e => e.Id == entitlementId);

        Assert.False(roleService.WasTransactionOpen);
        Assert.Equal(1, roleService.CallCount);

        Assert.Equal(RewardDeliveryStatus.Delivered, result.Status);
        Assert.Equal(RewardEntitlementStatus.Delivered, entitlement.Status);
        Assert.Equal(1, entitlement.AttemptCount);
    }

    [Fact]
    public async Task DeliverAsync_ShouldPersistFailure_WhenRoleAssignmentFails()
    {
        const ulong memberId = 5002UL;

        var entitlementId = await SeedRoleEntitlementAsync(memberId);

        var handler = CreateHandler(
            Context,
            new StaticRoleService(RoleOperationResult.Forbidden()));

        var result = await handler.DeliverAsync(entitlementId);

        await using var assertionContext = CreateDbContext();

        var entitlement = await assertionContext.RewardEntitlements
            .SingleAsync(e => e.Id == entitlementId);

        Assert.Equal(RewardDeliveryStatus.Failed, result.Status);
        Assert.Equal(RewardEntitlementStatus.Error, entitlement.Status);
        Assert.Equal(1, entitlement.AttemptCount);
        Assert.Contains(
            RoleOperationStatus.Forbidden.ToString(),
            entitlement.LastError);
    }

    [Fact]
    public async Task DeliverAsync_ShouldFinalizeOnlyOnce_WhenDeliveredConcurrently()
    {
        const ulong memberId = 5003UL;

        var entitlementId = await SeedRoleEntitlementAsync(memberId);

        await using var firstContext = CreateDbContext();
        await using var secondContext = CreateDbContext();

        var roleService = new BarrierRoleService(
            new AsyncBarrier(2));

        var firstHandler = CreateHandler(firstContext, roleService);
        var secondHandler = CreateHandler(secondContext, roleService);

        var results = await Task.WhenAll(
            firstHandler.DeliverAsync(entitlementId),
            secondHandler.DeliverAsync(entitlementId));

        await using var assertionContext = CreateDbContext();

        var entitlement = await assertionContext.RewardEntitlements
            .SingleAsync(e => e.Id == entitlementId);

        Assert.Equal(2, roleService.CallCount);

        Assert.Equal(
            1,
            results.Count(result => result.Status == RewardDeliveryStatus.Delivered));

        Assert.Equal(
            1,
            results.Count(result => result.Status == RewardDeliveryStatus.AlreadyFinalized));

        Assert.Equal(RewardEntitlementStatus.Delivered, entitlement.Status);
        Assert.Equal(1, entitlement.AttemptCount);
    }

    private async Task<long> SeedRoleEntitlementAsync(ulong memberId)
    {
        var member = Member.Create(
            new MemberIdentity(
                memberId,
                $"User{memberId}",
                $"User {memberId}",
                null,
                false));

        var entitlement = new RewardEntitlement
        {
            MemberId = memberId,
            SourceReference = $"role-reward-{memberId}",
            RewardType = RewardType.Role,
            RewardReference = RoleKey,
            Quantity = 1
        };

        Context.Members.Add(member);
        Context.RewardEntitlements.Add(entitlement);

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        return entitlement.Id;
    }

    private static RoleRewardDeliveryHandler CreateHandler(
        AppDbContext context,
        IRoleService roleService)
        => new(
            context,
            new RewardEntitlementRepository(context),
            roleService,
            NullLogger<RoleRewardDeliveryHandler>.Instance);

    private sealed class StaticRoleService(RoleOperationResult result)
        : IRoleService
    {
        public Task<RoleOperationResult> AssignConfiguredRoleByKeyAsync(
            ulong memberId,
            string roleKey,
            CancellationToken ct = default)
            => Task.FromResult(result);
    }

    private sealed class TransactionObservingRoleService(AppDbContext context)
        : IRoleService
    {
        public bool WasTransactionOpen { get; private set; }

        public int CallCount { get; private set; }

        public Task<RoleOperationResult> AssignConfiguredRoleByKeyAsync(
            ulong memberId,
            string roleKey,
            CancellationToken ct = default)
        {
            CallCount++;

            WasTransactionOpen =
                context.Database.CurrentTransaction is not null;

            return Task.FromResult(
                RoleOperationResult.Assigned(roleKey, 123UL));
        }
    }

    private sealed class BarrierRoleService(AsyncBarrier barrier)
        : IRoleService
    {
        private int _callCount;

        public int CallCount => _callCount;

        public async Task<RoleOperationResult> AssignConfiguredRoleByKeyAsync(
            ulong memberId,
            string roleKey,
            CancellationToken ct = default)
        {
            Interlocked.Increment(ref _callCount);

            await barrier.SignalAndWaitAsync(ct);

            return RoleOperationResult.Assigned(roleKey, 123UL);
        }
    }
}