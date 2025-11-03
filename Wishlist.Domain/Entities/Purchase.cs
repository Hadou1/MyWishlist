namespace Wishlist.Domain.Entities;

public class Purchase : BaseEntity
{
    public Guid ItemId { get; set; }
    public Guid? PurchasedByUserId { get; set; }
    public string? PurchasedByEmail { get; set; }
    public decimal? PaidPriceAmount { get; set; }
    public string? ReceiptUrl { get; set; }
    public bool IsVisibleToOwner { get; set; }

    public Item Item { get; set; } = default!;
    public User? PurchasedByUser { get; set; }
}
