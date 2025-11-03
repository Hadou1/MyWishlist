using Wishlist.Contracts.Gdpr;
using Wishlist.Contracts.Users;

namespace Wishlist.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> GetCurrentUserAsync(CancellationToken cancellationToken);
    Task DeleteCurrentUserAsync(CancellationToken cancellationToken);
    Task<GdprExportResponse> ExportAsync(CancellationToken cancellationToken);
}
