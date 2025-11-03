using Wishlist.Domain.Enums;

namespace Wishlist.Domain.Entities;

public class Item : BaseEntity
{
    public Guid ListId { get; set; }
    public string Title { get; set; } = default!;
    public string? Note { get; set; }
    public string? ProductUrl { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? PriceAmount { get; set; }
    public string? PriceCurrency { get; set; }
    public string? Retailer { get; set; }
    public string? VariantColor { get; set; }
    public string? VariantSize { get; set; }
    public int Quantity { get; set; } = 1;
    public ItemPriority Priority { get; set; } = ItemPriority.Medium;
    public LinkStatus LinkStatus { get; set; } = LinkStatus.Unknown;

    public List List { get; set; } = default!;
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
