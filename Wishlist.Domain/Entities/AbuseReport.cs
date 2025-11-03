namespace Wishlist.Domain.Entities;

public class AbuseReport : BaseEntity
{
    public string EntityType { get; set; } = default!;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = default!;
    public Guid? ActorId { get; set; }
    public string? ActorEmail { get; set; }
    public string? Message { get; set; }
}
