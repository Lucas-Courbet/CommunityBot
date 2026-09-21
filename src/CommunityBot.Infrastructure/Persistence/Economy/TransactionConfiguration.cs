using CommunityBot.Core.Economy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityBot.Infrastructure.Persistence.Economy;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(transaction => transaction.Amount)
            .IsRequired();

        builder.Property(transaction => transaction.Reason)
            .HasMaxLength(500);

        builder.Property(transaction => transaction.ActorId);
    }
}