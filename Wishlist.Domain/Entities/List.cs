using Wishlist.Domain.Enums;

namespace Wishlist.Domain.Entities;

public class List : BaseEntity
{
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = default!;
    public string? Occasion { get; set; }
    public DateOnly? EventDate { get; set; }
    public string? Description { get; set; }
    public ListVisibility Visibility { get; set; } = ListVisibility.Private;
    public string? CoverImageUrl { get; set; }
    public string? PasscodeHash { get; set; }
    public string? ShareSlug { get; set; }

    public User Owner { get; set; } = default!;
    public ICollection<ListMember> Members { get; set; } = new List<ListMember>();
    public ICollection<Item> Items { get; set; } = new List<Item>();
    public ICollection<Invite> Invites { get; set; } = new List<Invite>();
}
