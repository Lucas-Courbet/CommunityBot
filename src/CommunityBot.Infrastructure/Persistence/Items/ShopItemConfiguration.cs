using CommunityBot.Core.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityBot.Infrastructure.Persistence.Items;

public sealed class ShopItemConfiguration : IEntityTypeConfiguration<ShopItem>
{
    public void Configure(EntityTypeBuilder<ShopItem> builder)
    {
        builder.ToTable("shop_items");

        builder.HasKey(shopItem => shopItem.Id);

        builder.Property(shopItem => shopItem.Id)
            .HasMaxLength(100);

        builder.Property(shopItem => shopItem.Price)
            .IsRequired();

        builder.Property(shopItem => shopItem.Category)
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(shopItem => shopItem.IsEnabled)
            .HasDefaultValue(true);

        builder.HasOne(shopItem => shopItem.Item)
            .WithOne()
            .HasForeignKey<ShopItem>(shopItem => shopItem.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}