using Wishlist.Domain.Enums;

namespace Wishlist.Contracts.Invites;

public record InviteCreateRequest(string Email, ListRole Role);
