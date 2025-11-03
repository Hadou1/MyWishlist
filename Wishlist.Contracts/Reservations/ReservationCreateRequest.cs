namespace Wishlist.Contracts.Reservations;

public record ReservationCreateRequest(string? Message, string? Email, bool? IsVisibleToOwner);
