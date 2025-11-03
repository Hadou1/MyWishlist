using Wishlist.Domain.Enums;

namespace Wishlist.Contracts.Lists;

public record ListSummary(Guid Id, string Title, string? Occasion, DateOnly? EventDate, string? Description, ListVisibility Visibility, string? CoverImageUrl, string? ShareSlug, DateTime CreatedAt);
