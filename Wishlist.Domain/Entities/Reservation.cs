namespace Wishlist.Domain.Entities;

public class Reservation : BaseEntity
{
    public Guid ItemId { get; set; }
    public Guid? ReservedByUserId { get; set; }
    public string? ReservedByEmail { get; set; }
    public string? Message { get; set; }
    public bool IsVisibleToOwner { get; set; }

    public Item Item { get; set; } = default!;
    public User? ReservedByUser { get; set; }
}
