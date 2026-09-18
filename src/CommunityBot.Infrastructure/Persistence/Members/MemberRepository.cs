using CommunityBot.Application.Members;
using CommunityBot.Core.Members;
using Microsoft.EntityFrameworkCore;

namespace CommunityBot.Infrastructure.Persistence.Members;

/// <summary>
/// Entity Framework Core implementation of member persistence.
/// </summary>
public sealed class MemberRepository(AppDbContext context)
    : ABaseRepository<Member, ulong>(context), IMemberRepository
{
    /// <inheritdoc />
    public async Task<Member?> GetByUsernameAsync(
        string username,
        CancellationToken ct = default)
        => await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(member => member.Username == username, ct);

    /// <inheritdoc />
    public async Task<Member?> GetByIdForUpdateAsync(
        ulong memberId,
        CancellationToken ct = default)
    {
        var persistedMemberId = checked((long)memberId);

        return await DbSet
            .FromSqlInterpolated(
                $"""
                 SELECT *
                 FROM members
                 WHERE id = {persistedMemberId}
                 FOR UPDATE
                 """)
            .SingleOrDefaultAsync(ct);
    }
}