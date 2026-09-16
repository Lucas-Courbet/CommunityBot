using CommunityBot.Core.Activities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityBot.Infrastructure.Persistence.Activities;

public sealed class ActivityConsumptionConfiguration : IEntityTypeConfiguration<ActivityConsumption>
{
    public void Configure(EntityTypeBuilder<ActivityConsumption> builder)
    {
        builder.ToTable("activity_consumptions", table =>
        {
            table.HasCheckConstraint(
                "CK_activity_consumptions_attempt_count_non_negative",
                "attempt_count >= 0");
        });

        builder.HasKey(consumption => consumption.Id);

        builder.HasIndex(consumption => new
            {
                consumption.ActivityEventId,
                consumption.SubscriptionId
            })
            .IsUnique()
            .HasDatabaseName("ux_activity_consumptions_event_subscription");

        builder.HasIndex(consumption => new
            {
                consumption.CreatedAt,
                consumption.Id
            })
            .HasFilter("status = 'Pending'")
            .HasDatabaseName("ix_activity_consumptions_pending_queue");

        builder.HasOne(consumption => consumption.ActivityEvent)
            .WithMany()
            .HasForeignKey(consumption => consumption.ActivityEventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(consumption => consumption.Subscription)
            .WithMany()
            .HasForeignKey(consumption => consumption.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(consumption => consumption.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(ActivityConsumptionStatus.Pending)
            .IsRequired();

        builder.Property(consumption => consumption.AttemptCount)
            .HasDefaultValue(0)
            .IsRequired();
    }
}