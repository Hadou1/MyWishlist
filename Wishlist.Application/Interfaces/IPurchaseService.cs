using Wishlist.Contracts.Purchases;

namespace Wishlist.Application.Interfaces;

public interface IPurchaseService
{
    Task<PurchaseDto> CreateAsync(Guid itemId, PurchaseCreateRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PurchaseDto>> GetForItemAsync(Guid itemId, CancellationToken cancellationToken);
}
