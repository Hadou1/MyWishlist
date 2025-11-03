namespace Wishlist.Contracts.Reservations;

public record ReservationDto(Guid Id, Guid ItemId, string? ReservedBy, string? Message, bool IsVisibleToOwner, DateTime CreatedAt);
