using Wishlist.Domain.Enums;

namespace Wishlist.Contracts.Members;

public record ListMemberDto(Guid UserId, string Email, string? Name, ListRole Role, DateTime CreatedAt);
