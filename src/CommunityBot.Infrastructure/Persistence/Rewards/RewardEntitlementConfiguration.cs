using CommunityBot.Core.Rewards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityBot.Infrastructure.Persistence.Rewards;

public sealed class RewardEntitlementConfiguration : IEntityTypeConfiguration<RewardEntitlement>
{
    public void Configure(EntityTypeBuilder<RewardEntitlement> builder)
    {
        builder.ToTable("reward_entitlements", table =>
        {
            table.HasCheckConstraint(
                "CK_reward_entitlements_quantity_positive",
                "quantity > 0");

            table.HasCheckConstraint(
                "CK_reward_entitlements_attempt_count_non_negative",
                "attempt_count >= 0");
        });

        builder.HasKey(entitlement => entitlement.Id);

        builder.Property(entitlement => entitlement.SourceReference)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(entitlement => entitlement.RewardType)
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(entitlement => entitlement.RewardReference)
            .HasMaxLength(255);

        builder.Property(entitlement => entitlement.Quantity)
            .IsRequired();

        builder.Property(entitlement => entitlement.Status)
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(entitlement => entitlement.LastError)
            .HasMaxLength(1000);

        builder.HasOne(entitlement => entitlement.Member)
            .WithMany(member => member.RewardEntitlements)
            .HasForeignKey(entitlement => entitlement.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entitlement => entitlement.MemberId);
        builder.HasIndex(entitlement => entitlement.SourceReference);
    }
}