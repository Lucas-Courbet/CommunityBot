using CommunityBot.Core.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityBot.Infrastructure.Persistence.Items;

public sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable(
            "inventories",
            table => table.HasCheckConstraint(
                "CK_inventories_quantity_positive",
                "quantity > 0"));

        builder.HasKey(inventoryItem => inventoryItem.Id);

        builder.Property(inventoryItem => inventoryItem.Status)
            .HasMaxLength(50)
            .HasConversion<string>()
            .HasDefaultValue(ItemStatus.Active)
            .IsRequired();

        builder.HasOne(inventoryItem => inventoryItem.Item)
            .WithMany()
            .HasForeignKey(inventoryItem => inventoryItem.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(inventoryItem => inventoryItem.Member)
            .WithMany(member => member.Inventory)
            .HasForeignKey(inventoryItem => inventoryItem.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(inventoryItem => inventoryItem.MemberId)
            .HasDatabaseName("ix_inventories_member_id");

        builder.HasIndex(inventoryItem => new { inventoryItem.MemberId, inventoryItem.ItemId })
            .IsUnique()
            .HasFilter("status = 'Active'")
            .HasDatabaseName("ux_inventories_member_item_active");
    }
}