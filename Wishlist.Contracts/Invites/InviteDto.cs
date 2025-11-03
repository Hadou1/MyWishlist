using Wishlist.Domain.Enums;

namespace Wishlist.Contracts.Invites;

public record InviteDto(Guid Id, Guid ListId, string Email, ListRole Role, string Token, DateTime ExpiresAt, DateTime? AcceptedAt, DateTime CreatedAt);
