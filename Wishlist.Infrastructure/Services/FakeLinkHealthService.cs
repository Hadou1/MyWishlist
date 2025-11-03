using Wishlist.Application.Interfaces;

namespace Wishlist.Infrastructure.Services;

public class FakeLinkHealthService : ILinkHealthService
{
    public Task MarkAsync(Guid itemId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
