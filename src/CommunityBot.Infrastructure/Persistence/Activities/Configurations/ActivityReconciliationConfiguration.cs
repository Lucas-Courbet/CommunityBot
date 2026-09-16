using CommunityBot.Core.Activities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityBot.Infrastructure.Persistence.Activities.Configurations;

public sealed class ActivityReconciliationConfiguration
    : IEntityTypeConfiguration<ActivityReconciliation>
{
    public void Configure(EntityTypeBuilder<ActivityReconciliation> builder)
    {
        builder.ToTable("activity_reconciliations");

        builder.HasKey(reconciliation => reconciliation.ActivityEventId);

        builder.Property(reconciliation => reconciliation.ActivityEventId)
            .ValueGeneratedNever();

        builder.Property(reconciliation => reconciliation.CreatedAt)
            .IsRequired();

        builder.HasOne(reconciliation => reconciliation.ActivityEvent)
            .WithOne()
            .HasForeignKey<ActivityReconciliation>(
                reconciliation => reconciliation.ActivityEventId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}