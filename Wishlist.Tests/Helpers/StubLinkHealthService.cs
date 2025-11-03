using Wishlist.Application.Interfaces;

namespace Wishlist.Tests.Helpers;

public class StubLinkHealthService : ILinkHealthService
{
    public List<Guid> MarkedItems { get; } = new();

    public Task MarkAsync(Guid itemId, CancellationToken cancellationToken)
    {
        MarkedItems.Add(itemId);
        return Task.CompletedTask;
    }
}
