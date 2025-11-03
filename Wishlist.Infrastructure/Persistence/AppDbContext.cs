using Microsoft.EntityFrameworkCore;
using Wishlist.Application.Interfaces;
using Wishlist.Domain.Entities;

namespace Wishlist.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<List> Lists => Set<List>();
    public DbSet<ListMember> ListMembers => Set<ListMember>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<Invite> Invites => Set<Invite>();
    public DbSet<AbuseReport> AbuseReports => Set<AbuseReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
