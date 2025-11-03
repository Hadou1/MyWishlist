using Wishlist.Domain.Enums;

namespace Wishlist.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string? Name { get; set; }
    public string? Locale { get; set; }
    public string? Country { get; set; }
    public string? Currency { get; set; }

    public ICollection<List> OwnedLists { get; set; } = new List<List>();
    public ICollection<ListMember> Memberships { get; set; } = new List<ListMember>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
