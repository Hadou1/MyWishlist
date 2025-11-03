using Wishlist.Application.Common;
using Wishlist.Contracts.Admin;
using Wishlist.Contracts.Common;

namespace Wishlist.Application.Interfaces;

public interface IAdminService
{
    Task<PagedResult<AdminUserDto>> GetUsersAsync(QueryParameters query, CancellationToken cancellationToken);
    Task<PagedResult<AdminListDto>> GetListsAsync(QueryParameters query, CancellationToken cancellationToken);
    Task ReportAsync(AbuseReportRequest request, CancellationToken cancellationToken);
    Task ModerateAsync(ModerationRequest request, CancellationToken cancellationToken);
}
