namespace Wishlist.Contracts.Admin;

public record ModerationRequest(string EntityType, Guid EntityId, string Action);
