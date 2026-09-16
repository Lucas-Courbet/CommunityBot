using CommunityBot.Core.Activities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityBot.Infrastructure.Persistence.Activities.Configurations;

public sealed class ActivityCaptureGateConfiguration : IEntityTypeConfiguration<ActivityCaptureGate>
{
    public void Configure(EntityTypeBuilder<ActivityCaptureGate> builder)
    {
        builder.ToTable("activity_capture_gates");

        builder.HasKey(gate => gate.EventType);

        builder.Property(gate => gate.EventType)
            .HasConversion<string>()
            .HasMaxLength(100)
            .HasColumnName("event_type")
            .IsRequired();

        builder.HasData(
            new ActivityCaptureGate
            {
                EventType = ActivityEventType.ShopPurchaseCompleted
            });
    }
}