using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishlist.Domain.Entities;

namespace Wishlist.Infrastructure.Persistence.Configurations;

public class ListConfiguration : IEntityTypeConfiguration<List>
{
    public void Configure(EntityTypeBuilder<List> builder)
    {
        builder.HasOne(l => l.Owner)
            .WithMany(u => u.OwnedLists)
            .HasForeignKey(l => l.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(l => l.Title).IsRequired().HasMaxLength(120);
        builder.Property(l => l.Occasion).HasMaxLength(80);
        builder.Property(l => l.EventDate).HasColumnType("date");
        builder.HasIndex(l => l.OwnerId);
        builder.HasIndex(l => l.ShareSlug).IsUnique(false);
    }
}
