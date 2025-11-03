namespace Wishlist.Application.Interfaces;

public interface ILinkHealthService
{
    Task MarkAsync(Guid itemId, CancellationToken cancellationToken);
}
