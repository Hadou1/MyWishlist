namespace Wishlist.Contracts.Lists;

public record ListCreateRequest(string Title, string? Occasion, DateOnly? EventDate, string? Description, string? CoverImageUrl);
