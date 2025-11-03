using Microsoft.EntityFrameworkCore;
using Wishlist.Domain.Entities;

namespace Wishlist.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<List> Lists { get; }
    DbSet<ListMember> ListMembers { get; }
    DbSet<Item> Items { get; }
    DbSet<Reservation> Reservations { get; }
    DbSet<Purchase> Purchases { get; }
    DbSet<Invite> Invites { get; }
    DbSet<AbuseReport> AbuseReports { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
