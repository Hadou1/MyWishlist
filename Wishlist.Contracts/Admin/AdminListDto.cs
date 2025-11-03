namespace Wishlist.Contracts.Admin;

public record AdminListDto(Guid Id, string Title, string OwnerEmail, DateTime CreatedAt, string Visibility);
