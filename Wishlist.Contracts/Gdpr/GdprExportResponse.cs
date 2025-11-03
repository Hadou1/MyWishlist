using Wishlist.Contracts.Items;
using Wishlist.Contracts.Lists;

namespace Wishlist.Contracts.Gdpr;

public record GdprExportResponse(UserSnapshot User, IReadOnlyCollection<ListSnapshot> Lists);

public record UserSnapshot(Guid Id, string Email, string? Name, string? Locale, string? Country, string? Currency, DateTime CreatedAt);

public record ListSnapshot(ListSummary List, IReadOnlyCollection<ItemSnapshot> Items, IReadOnlyCollection<ReservationSnapshot> Reservations, IReadOnlyCollection<PurchaseSnapshot> Purchases);

public record ItemSnapshot(Guid Id, Guid ListId, string Title, string? Note, string? ProductUrl, string? ImageUrl, decimal? PriceAmount, string? PriceCurrency, string? Retailer, string? VariantColor, string? VariantSize, int Quantity, int Priority, int LinkStatus, DateTime CreatedAt);

public record ReservationSnapshot(Guid Id, Guid ItemId, Guid? ReservedByUserId, string? ReservedByEmail, string? Message, bool IsVisibleToOwner, DateTime CreatedAt);

public record PurchaseSnapshot(Guid Id, Guid ItemId, Guid? PurchasedByUserId, string? PurchasedByEmail, decimal? PaidPriceAmount, string? ReceiptUrl, bool IsVisibleToOwner, DateTime CreatedAt);
