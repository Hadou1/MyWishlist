using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishlist.Domain.Entities;

namespace Wishlist.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasOne(r => r.Item)
            .WithMany(i => i.Reservations)
            .HasForeignKey(r => r.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(r => r.ItemId);
    }
}
