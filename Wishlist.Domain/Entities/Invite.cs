using Wishlist.Domain.Enums;

namespace Wishlist.Domain.Entities;

public class Invite : BaseEntity
{
    public Guid ListId { get; set; }
    public string Email { get; set; } = default!;
    public ListRole Role { get; set; } = ListRole.Viewer;
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }

    public List List { get; set; } = default!;
}
