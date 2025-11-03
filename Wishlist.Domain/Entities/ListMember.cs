using Wishlist.Domain.Enums;

namespace Wishlist.Domain.Entities;

public class ListMember
{
    public Guid ListId { get; set; }
    public Guid UserId { get; set; }
    public ListRole Role { get; set; } = ListRole.Viewer;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List List { get; set; } = default!;
    public User User { get; set; } = default!;
}
