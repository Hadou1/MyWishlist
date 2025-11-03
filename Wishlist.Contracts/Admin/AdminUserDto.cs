namespace Wishlist.Contracts.Admin;

public record AdminUserDto(Guid Id, string Email, string? Name, DateTime CreatedAt);
