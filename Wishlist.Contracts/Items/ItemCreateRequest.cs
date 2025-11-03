using Wishlist.Domain.Enums;

namespace Wishlist.Contracts.Items;

public record ItemCreateRequest(Guid ListId, string Title, string? Note, string? ProductUrl, string? ImageUrl, decimal? PriceAmount, string? PriceCurrency, string? Retailer, string? VariantColor, string? VariantSize, int Quantity, ItemPriority Priority);
