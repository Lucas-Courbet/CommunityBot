using CommunityBot.Core.Goals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityBot.Infrastructure.Persistence.Goals;

public sealed class CommunityGoalConfiguration : IEntityTypeConfiguration<CommunityGoal>
{
    public void Configure(EntityTypeBuilder<CommunityGoal> builder)
    {
        builder.ToTable("community_goals", table =>
        {
            table.HasCheckConstraint(
                "CK_community_goals_target_count_positive",
                "target_count > 0");

            table.HasCheckConstraint(
                "CK_community_goals_current_count_valid",
                "current_count >= 0 AND current_count <= target_count");

            table.HasCheckConstraint(
                "CK_community_goals_valid_window",
                "ends_at > starts_at");

            table.HasCheckConstraint(
                "CK_community_goals_id_not_blank",
                "length(btrim(id)) > 0");

            table.HasCheckConstraint(
                "CK_community_goals_title_not_blank",
                "length(btrim(title)) > 0");
        });

        builder.HasKey(goal => goal.Id);

        builder.Property(goal => goal.Id)
            .HasMaxLength(100)
            .ValueGeneratedNever();

        builder.Property(goal => goal.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(goal => goal.TargetCount)
            .IsRequired();

        builder.Property(goal => goal.CurrentCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(goal => goal.StartsAt)
            .IsRequired();

        builder.Property(goal => goal.EndsAt)
            .IsRequired();
    }
}