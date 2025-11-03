using Wishlist.Contracts.Reservations;

namespace Wishlist.Application.Interfaces;

public interface IReservationService
{
    Task<ReservationDto> CreateAsync(Guid itemId, ReservationCreateRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ReservationDto>> GetForItemAsync(Guid itemId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid reservationId, CancellationToken cancellationToken);
}
