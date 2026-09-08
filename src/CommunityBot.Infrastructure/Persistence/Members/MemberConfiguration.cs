using CommunityBot.Core.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityBot.Infrastructure.Persistence.Members;

/// <summary>
/// Configures relational persistence for <see cref="Member"/>.
/// </summary>
public sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members");

        builder.HasKey(member => member.Id);

        builder.Property(member => member.Id)
            .ValueGeneratedNever();

        builder.Property(member => member.Username)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(member => member.DisplayName)
            .HasMaxLength(255);

        builder.Property(member => member.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(member => member.CurrencyBalance)
            .HasDefaultValue(0);

        builder.HasMany(member => member.Transactions)
            .WithOne(transaction => transaction.Member)
            .HasForeignKey(transaction => transaction.MemberId);
    }
}