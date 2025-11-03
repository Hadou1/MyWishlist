namespace Wishlist.Contracts.Auth;

public record AuthResponse(Guid UserId, string AccessToken, DateTime ExpiresAt);
