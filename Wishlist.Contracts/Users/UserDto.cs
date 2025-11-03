namespace Wishlist.Contracts.Users;

public record UserDto(Guid Id, string Email, string? Name, string? Locale, string? Country, string? Currency, DateTime CreatedAt);
