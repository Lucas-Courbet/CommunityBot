using CommunityBot.Core.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityBot.Infrastructure.Persistence.Members;

/// <summary>
/// Configures the relational persistence mapping for <see cref="Member"/>.
/// </summary>
public sealed class MemberConfiguration
    : IEntityTypeConfiguration<Member>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members");

        builder.HasKey(member => member.Id);

        // Discord supplies the snowflake identifier.
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
    }
}