using Wishlist.Contracts.Invites;

namespace Wishlist.Application.Interfaces;

public interface IInviteService
{
    Task<InviteDto> CreateAsync(Guid listId, InviteCreateRequest request, CancellationToken cancellationToken);
    Task<InviteDto> AcceptAsync(InviteAcceptRequest request, CancellationToken cancellationToken);
    Task<InviteDto?> GetByTokenAsync(string token, CancellationToken cancellationToken);
}
