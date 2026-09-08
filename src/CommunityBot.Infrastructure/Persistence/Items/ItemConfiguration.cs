using CommunityBot.Core.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityBot.Infrastructure.Persistence.Items;

public sealed class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("items");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .HasMaxLength(100);

        builder.Property(item => item.Label)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(item => item.IsStackable)
            .IsRequired();

        builder.Property(item => item.GrantedRoleKey)
            .HasMaxLength(128)
            .IsRequired(false);
    }
}