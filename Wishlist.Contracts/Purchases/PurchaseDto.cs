namespace Wishlist.Contracts.Purchases;

public record PurchaseDto(Guid Id, Guid ItemId, string? PurchasedBy, decimal? PaidPriceAmount, string? ReceiptUrl, bool IsVisibleToOwner, DateTime CreatedAt);
