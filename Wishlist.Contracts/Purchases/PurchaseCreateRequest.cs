namespace Wishlist.Contracts.Purchases;

public record PurchaseCreateRequest(decimal? PaidPriceAmount, string? ReceiptUrl, string? Email, bool? IsVisibleToOwner);
