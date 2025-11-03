using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wishlist.Domain.Entities;

namespace Wishlist.Infrastructure.Persistence.Configurations;

public class ListMemberConfiguration : IEntityTypeConfiguration<ListMember>
{
    public void Configure(EntityTypeBuilder<ListMember> builder)
    {
        builder.HasKey(lm => new { lm.ListId, lm.UserId });
        builder.HasOne(lm => lm.List)
            .WithMany(l => l.Members)
            .HasForeignKey(lm => lm.ListId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(lm => lm.User)
            .WithMany(u => u.Memberships)
            .HasForeignKey(lm => lm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
