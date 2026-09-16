using CommunityBot.Core.Activities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityBot.Infrastructure.Persistence.Activities.Configurations;

public sealed class ActivitySubscriptionConfiguration : IEntityTypeConfiguration<ActivitySubscription>
{
    public void Configure(EntityTypeBuilder<ActivitySubscription> builder)
    {
        builder.ToTable("activity_subscriptions", table =>
        {
            table.HasCheckConstraint(
                "CK_activity_subscriptions_valid_window",
                "capture_until > capture_from");

            table.HasCheckConstraint(
                "CK_activity_subscriptions_context_reference_not_blank",
                "length(btrim(context_reference)) > 0");
        });

        builder.HasKey(subscription => subscription.Id);

        builder.HasIndex(subscription => new
            {
                subscription.ConsumerType,
                subscription.ContextReference,
                subscription.EventType
            })
            .IsUnique()
            .HasDatabaseName("ux_activity_subscriptions_consumer_context_event_type");

        builder.HasOne(subscription => subscription.CaptureGate)
            .WithMany()
            .HasForeignKey(subscription => subscription.EventType)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(subscription => subscription.EventType)
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(subscription => subscription.ConsumerType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(subscription => subscription.ContextReference)
            .HasMaxLength(150)
            .IsRequired();
    }
}