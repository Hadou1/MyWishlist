using Wishlist.Domain.Enums;

namespace Wishlist.Contracts.Lists;

public record ListUpdateRequest(string? Title, string? Occasion, DateOnly? EventDate, string? Description, ListVisibility? Visibility, string? CoverImageUrl, bool? RemovePasscode, string? Passcode);
