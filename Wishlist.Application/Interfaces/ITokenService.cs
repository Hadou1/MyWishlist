using Wishlist.Domain.Entities;

namespace Wishlist.Application.Interfaces;

public interface ITokenService
{
    (string AccessToken, DateTime ExpiresAt) CreateAccessToken(User user, IEnumerable<string> roles);
}
