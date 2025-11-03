using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishlist.Domain.Entities;

namespace Wishlist.Infrastructure.Persistence.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.Property(i => i.Title).IsRequired().HasMaxLength(160);
        builder.Property(i => i.PriceAmount).HasColumnType("numeric(12,2)");
        builder.Property(i => i.PriceCurrency).HasMaxLength(3);
        builder.HasIndex(i => i.ListId);
    }
}
