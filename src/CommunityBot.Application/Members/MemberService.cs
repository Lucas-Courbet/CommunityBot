using CommunityBot.Core.Members;
using Microsoft.Extensions.Logging;

namespace CommunityBot.Application.Members;

/// <summary>
/// Coordinates member lifecycle operations between the domain
/// and the persistence layer.
/// </summary>
public sealed class MemberService(
    IMemberRepository memberRepository,
    ILogger<MemberService> logger)
    : IMemberService
{
    /// <inheritdoc />
    public async Task<MemberSynchronizationStatus> SynchronizeAsync(
        MemberIdentity identity,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(identity);

        var member = await memberRepository.GetByIdAsync(identity.Id, ct);

        if (member is null)
        {
            member = Member.Create(identity);
            memberRepository.Add(member);
            await memberRepository.SaveChangesAsync(ct);

            logger.LogInformation(
                "Created member {Username} ({MemberId})",
                identity.DisplayName ?? identity.Username,
                identity.Id);

            return MemberSynchronizationStatus.Created;
        }

        if (!member.IsActive)
        {
            member.Reactivate(identity);
            await memberRepository.SaveChangesAsync(ct);

            logger.LogInformation(
                "Reactivated member {Username} ({MemberId})",
                identity.DisplayName ?? identity.Username,
                identity.Id);

            return MemberSynchronizationStatus.Reactivated;
        }

        if (!member.SynchronizeIdentity(identity))
            return MemberSynchronizationStatus.Unchanged;

        await memberRepository.SaveChangesAsync(ct);

        logger.LogInformation(
            "Updated member identity {Username} ({MemberId})",
            identity.DisplayName ?? identity.Username,
            identity.Id);

        return MemberSynchronizationStatus.Updated;
    }

    /// <inheritdoc />
    public async Task<MemberDeactivationStatus> DeactivateAsync(
        ulong id,
        CancellationToken ct = default)
    {
        var member = await memberRepository.GetByIdAsync(id, ct);

        if (member is null)
        {
            logger.LogWarning("Attempted to deactivate unknown member {MemberId}",
                id);

            return MemberDeactivationStatus.NotFound;
        }

        if (!member.IsActive)
            return MemberDeactivationStatus.AlreadyInactive;

        member.Deactivate();
        await memberRepository.SaveChangesAsync(ct);

        logger.LogInformation(
            "Deactivated member {Username} ({MemberId})",
            member.DisplayName ?? member.Username,
            member.Id);

        return MemberDeactivationStatus.Deactivated;
    }

    /// <inheritdoc />
    public Task<Member?> GetByIdAsync(
        ulong id,
        CancellationToken ct = default)
        => memberRepository.GetByIdAsync(id, ct);

    /// <inheritdoc />
    public Task<Member?> GetByUsernameAsync(
        string username,
        CancellationToken ct = default)
        => memberRepository.GetByUsernameAsync(username, ct);

    /// <inheritdoc />
    public Task<bool> ExistsAsync(
        ulong id,
        CancellationToken ct = default)
        => memberRepository.ExistsAsync(id, ct);
}