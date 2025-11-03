namespace Wishlist.Contracts.Auth;

public record RegisterRequest(string Email, string Password, string? Name, string? Locale, string? Country, string? Currency);
