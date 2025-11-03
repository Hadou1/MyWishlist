using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Wishlist.Application.Interfaces;
using Wishlist.Application.Security;
using Wishlist.Application.Services;
using Wishlist.Application.Validators;
using Wishlist.Contracts.Auth;
using Wishlist.Domain.Entities;
using Wishlist.Tests.Helpers;
using Xunit;

namespace Wishlist.Tests;

public class AuthServiceTests
{
    private class TestTokenService : ITokenService
    {
        public (string AccessToken, DateTime ExpiresAt) CreateAccessToken(User user, IEnumerable<string> roles) => ("token", DateTime.UtcNow.AddMinutes(5));
    }

    [Fact]
    public async Task Should_Register_User()
    {
        using var fixture = new TestFixture();
        var service = new AuthService(fixture.Context, new RegisterRequestValidator(), new LoginRequestValidator(), new Pbkdf2PasswordHasher(), new TestTokenService(), fixture.Mapper, NullLogger<AuthService>.Instance);
        var user = await service.RegisterAsync(new RegisterRequest("user@local", "Passw0rd!", "User", null, null, null), CancellationToken.None);
        user.Email.Should().Be("user@local");
    }

    [Fact]
    public async Task Should_Not_Login_With_Invalid_Password()
    {
        using var fixture = new TestFixture();
        var hasher = new Pbkdf2PasswordHasher();
        var auth = new AuthService(fixture.Context, new RegisterRequestValidator(), new LoginRequestValidator(), hasher, new TestTokenService(), fixture.Mapper, NullLogger<AuthService>.Instance);
        await auth.RegisterAsync(new RegisterRequest("user@local", "Passw0rd!", null, null, null, null), CancellationToken.None);

        var act = async () => await auth.LoginAsync(new LoginRequest("user@local", "wrong"), CancellationToken.None);
        await act.Should().ThrowAsync<Wishlist.Application.Models.AppException>();
    }
}
