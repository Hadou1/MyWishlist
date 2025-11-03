using Wishlist.Domain.Enums;

namespace Wishlist.Contracts.Members;

public record ListMemberCreateRequest(Guid UserId, ListRole Role);
