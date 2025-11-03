using Wishlist.Application.Common;
using Wishlist.Contracts.Common;
using Wishlist.Contracts.Items;

namespace Wishlist.Application.Interfaces;

public interface IItemService
{
    Task<ItemDto> CreateAsync(ItemCreateRequest request, CancellationToken cancellationToken);
    Task<ItemDto> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<ItemDto>> GetByListAsync(Guid listId, QueryParameters query, string? status, CancellationToken cancellationToken);
    Task<ItemDto> UpdateAsync(Guid id, ItemUpdateRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
