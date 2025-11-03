namespace Wishlist.Contracts.Admin;

public record AbuseReportRequest(string EntityType, Guid EntityId, string Action, string? Message);
