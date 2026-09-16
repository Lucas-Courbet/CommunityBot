using CommunityBot.Core.Activities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityBot.Infrastructure.Persistence.Activities.Configurations;

public sealed class ActivityCaptureIncidentConfiguration
    : IEntityTypeConfiguration<ActivityCaptureIncident>
{
    public void Configure(EntityTypeBuilder<ActivityCaptureIncident> builder)
    {
        builder.ToTable("activity_capture_incidents", table =>
        {
            table.HasCheckConstraint(
                "CK_activity_capture_incidents_occurrence_count_positive",
                "occurrence_count > 0");

            table.HasCheckConstraint(
                "CK_activity_capture_incidents_source_reference_not_blank",
                "source_reference IS NULL OR length(btrim(source_reference)) > 0");

            table.HasCheckConstraint(
                "CK_activity_capture_incidents_nominal_error_not_blank",
                "length(btrim(nominal_error)) > 0");

            table.HasCheckConstraint(
                "CK_activity_capture_incidents_conservative_error_not_blank",
                "length(btrim(conservative_error)) > 0");
        });

        builder.HasKey(incident => incident.Id);

        builder.HasIndex(incident => incident.DetectedAt);

        builder.Property(incident => incident.EventType)
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(incident => incident.SourceReference)
            .HasMaxLength(150);

        builder.Property(incident => incident.NominalError)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(incident => incident.ConservativeError)
            .HasColumnType("text")
            .IsRequired();
    }
}