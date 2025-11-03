using Wishlist.Application.Interfaces;

namespace Wishlist.Tests.Helpers;

public class TestCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public bool IsAuthenticated { get; set; }
    public bool IsAdmin { get; set; }
}
