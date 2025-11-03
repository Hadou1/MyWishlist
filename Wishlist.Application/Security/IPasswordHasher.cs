namespace Wishlist.Application.Security;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyHashedPassword(string hash, string password);
}
