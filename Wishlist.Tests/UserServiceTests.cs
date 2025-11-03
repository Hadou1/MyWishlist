using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using Wishlist.Application.Services;
using Wishlist.Domain.Entities;
using Wishlist.Domain.Enums;
using Wishlist.Tests.Helpers;
using Xunit;

namespace Wishlist.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task Export_ShouldIncludeListsAndItems()
    {
        using var fixture = new TestFixture();
        var userId = Guid.NewGuid();
        var currentUser = new TestCurrentUserService { UserId = userId, IsAuthenticated = true };
        var mapper = fixture.Mapper;
        var service = new UserService(fixture.Context, currentUser, mapper, NullLogger<UserService>.Instance);

        var user = new User { Id = userId, Email = "user@local", PasswordHash = "hash", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        fixture.Context.Users.Add(user);
        var list = new List { OwnerId = userId, Title = "List", Visibility = ListVisibility.Private, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        fixture.Context.Lists.Add(list);
        fixture.Context.ListMembers.Add(new ListMember { List = list, UserId = userId, Role = ListRole.Owner, CreatedAt = DateTime.UtcNow });
        var item = new Item { List = list, Title = "Item", Quantity = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        fixture.Context.Items.Add(item);
        fixture.Context.Reservations.Add(new Reservation { Item = item, ReservedByUserId = userId, ReservedByEmail = "user@local", IsVisibleToOwner = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        fixture.Context.Purchases.Add(new Purchase { Item = item, PurchasedByUserId = userId, PurchasedByEmail = "user@local", IsVisibleToOwner = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await fixture.Context.SaveChangesAsync();

        var export = await service.ExportAsync(CancellationToken.None);
        export.Lists.Should().ContainSingle();
        export.Lists.First().Items.Should().ContainSingle();
        export.Lists.First().Reservations.Should().ContainSingle();
    }
}
