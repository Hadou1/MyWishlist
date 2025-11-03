using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishlist.Domain.Entities;

namespace Wishlist.Infrastructure.Persistence.Configurations;

public class InviteConfiguration : IEntityTypeConfiguration<Invite>
{
    public void Configure(EntityTypeBuilder<Invite> builder)
    {
        builder.HasIndex(i => new { i.ListId, i.Token }).IsUnique();
        builder.Property(i => i.Email).IsRequired().HasMaxLength(320);
        builder.HasOne(i => i.List)
            .WithMany(l => l.Invites)
            .HasForeignKey(i => i.ListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
