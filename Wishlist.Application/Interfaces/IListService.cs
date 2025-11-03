using Wishlist.Application.Common;
using Wishlist.Contracts.Common;
using Wishlist.Contracts.Lists;
using Wishlist.Contracts.Members;

namespace Wishlist.Application.Interfaces;

public interface IListService
{
    Task<ListSummary> CreateAsync(ListCreateRequest request, CancellationToken cancellationToken);
    Task<PagedResult<ListSummary>> GetAsync(QueryParameters query, string? visibility, string? occasion, DateOnly? from, DateOnly? to, CancellationToken cancellationToken);
    Task<ListDetail> GetByIdAsync(Guid id, string? passcode, CancellationToken cancellationToken);
    Task<ListSummary> UpdateAsync(Guid id, ListUpdateRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ListMemberDto>> GetMembersAsync(Guid listId, CancellationToken cancellationToken);
    Task<ListMemberDto> AddMemberAsync(Guid listId, ListMemberCreateRequest request, CancellationToken cancellationToken);
    Task RemoveMemberAsync(Guid listId, Guid userId, CancellationToken cancellationToken);
    Task<ShareLinkResponse> GenerateShareLinkAsync(Guid listId, CancellationToken cancellationToken);
    Task SetPasscodeAsync(Guid listId, string? passcode, CancellationToken cancellationToken);
}
