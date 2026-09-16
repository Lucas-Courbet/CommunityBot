using CommunityBot.Core.Activities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityBot.Infrastructure.Persistence.Activities;

public sealed class ActivityEventConfiguration : IEntityTypeConfiguration<ActivityEvent>
{
    public void Configure(EntityTypeBuilder<ActivityEvent> builder)
    {
        builder.ToTable("activity_events", table =>
        {
            table.HasCheckConstraint(
                "CK_activity_events_occurrence_count_positive",
                "occurrence_count > 0");

            table.HasCheckConstraint(
                "CK_activity_events_contract_version_positive",
                "contract_version > 0");

            table.HasCheckConstraint(
                "CK_activity_events_event_id_not_empty",
                "event_id <> '00000000-0000-0000-0000-000000000000'::uuid");

            table.HasCheckConstraint(
                "CK_activity_events_source_reference_not_blank",
                "source_reference IS NULL OR length(btrim(source_reference)) > 0");
        });

        builder.HasKey(activityEvent => activityEvent.Id);

        builder.HasIndex(activityEvent => activityEvent.EventId)
            .IsUnique()
            .HasDatabaseName("ux_activity_events_event_id");

        builder.HasOne<ActivityCaptureGate>()
            .WithMany()
            .HasForeignKey(activityEvent => activityEvent.EventType)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(activityEvent => activityEvent.Member)
            .WithMany()
            .HasForeignKey(activityEvent => activityEvent.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(activityEvent => activityEvent.EventId)
            .IsRequired();

        builder.Property(activityEvent => activityEvent.EventType)
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(activityEvent => activityEvent.MemberId)
            .IsRequired();

        builder.Property(activityEvent => activityEvent.OccurredAt)
            .IsRequired();

        builder.Property(activityEvent => activityEvent.OccurrenceCount)
            .IsRequired();

        builder.Property(activityEvent => activityEvent.CapturedAt)
            .IsRequired();

        builder.Property(activityEvent => activityEvent.ContractVersion)
            .IsRequired();

        builder.Property(activityEvent => activityEvent.SourceReference)
            .HasMaxLength(150);
    }
}