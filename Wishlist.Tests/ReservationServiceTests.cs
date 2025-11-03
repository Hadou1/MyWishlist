using FluentAssertions;
using System.Linq;
using Wishlist.Application.Services;
using Wishlist.Contracts.Reservations;
using Wishlist.Domain.Entities;
using Wishlist.Domain.Enums;
using Wishlist.Tests.Helpers;
using Xunit;

namespace Wishlist.Tests;

public class ReservationServiceTests
{
    [Fact]
    public async Task Should_AllowOnlyOneReservationPerItem()
    {
        using var fixture = new TestFixture();
        var currentUser = new TestCurrentUserService { UserId = Guid.NewGuid(), Email = "user@local", IsAuthenticated = true };
        var service = new ReservationService(fixture.Context, currentUser, fixture.Mapper);

        var list = new List { OwnerId = currentUser.UserId!.Value, Title = "Test", Visibility = ListVisibility.Private, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        fixture.Context.Lists.Add(list);
        fixture.Context.ListMembers.Add(new ListMember { List = list, UserId = currentUser.UserId.Value, Role = ListRole.Owner, CreatedAt = DateTime.UtcNow });
        var item = new Item { List = list, Title = "Item", Quantity = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        fixture.Context.Items.Add(item);
        await fixture.Context.SaveChangesAsync();

        await service.CreateAsync(item.Id, new ReservationCreateRequest(null, null, null), CancellationToken.None);
        var act = async () => await service.CreateAsync(item.Id, new ReservationCreateRequest(null, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<Wishlist.Application.Models.ValidationAppException>();
    }

    [Fact]
    public async Task Should_MaskOwnerWhenNotVisible()
    {
        using var fixture = new TestFixture();
        var ownerId = Guid.NewGuid();
        var reserverId = Guid.NewGuid();
        var ownerService = new TestCurrentUserService { UserId = ownerId, Email = "owner@local", IsAuthenticated = true };
        var reservationService = new ReservationService(fixture.Context, ownerService, fixture.Mapper);

        var list = new List { OwnerId = ownerId, Title = "List", Visibility = ListVisibility.Private, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        fixture.Context.Lists.Add(list);
        fixture.Context.ListMembers.Add(new ListMember { List = list, UserId = ownerId, Role = ListRole.Owner, CreatedAt = DateTime.UtcNow });
        fixture.Context.ListMembers.Add(new ListMember { List = list, UserId = reserverId, Role = ListRole.Viewer, CreatedAt = DateTime.UtcNow });
        var item = new Item { List = list, Title = "Item", Quantity = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        fixture.Context.Items.Add(item);
        fixture.Context.Reservations.Add(new Reservation
        {
            Item = item,
            ReservedByUserId = reserverId,
            ReservedByEmail = "friend@local",
            IsVisibleToOwner = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();

        var reservations = await reservationService.GetForItemAsync(item.Id, CancellationToken.None);
        reservations.Should().ContainSingle();
        reservations.First().ReservedBy.Should().BeNull();
    }
    [Fact]
    public async Task PurchaseMasking_ShouldHideForOwner()
    {
        using var fixture = new TestFixture();
        var ownerId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();
        fixture.Context.Users.Add(new User { Id = ownerId, Email = "owner@local", PasswordHash = "hash", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        fixture.Context.Users.Add(new User { Id = buyerId, Email = "buyer@local", PasswordHash = "hash", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        var list = new List { OwnerId = ownerId, Title = "List", Visibility = ListVisibility.Private, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        fixture.Context.Lists.Add(list);
        fixture.Context.ListMembers.Add(new ListMember { List = list, UserId = ownerId, Role = ListRole.Owner, CreatedAt = DateTime.UtcNow });
        fixture.Context.ListMembers.Add(new ListMember { List = list, UserId = buyerId, Role = ListRole.Viewer, CreatedAt = DateTime.UtcNow });
        var item = new Item { List = list, Title = "Item", Quantity = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        fixture.Context.Items.Add(item);
        fixture.Context.Purchases.Add(new Purchase
        {
            Item = item,
            PurchasedByUserId = buyerId,
            PurchasedByEmail = "buyer@local",
            IsVisibleToOwner = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();

        var ownerService = new TestCurrentUserService { UserId = ownerId, Email = "owner@local", IsAuthenticated = true };
        var purchaseService = new PurchaseService(fixture.Context, ownerService);

        var purchases = await purchaseService.GetForItemAsync(item.Id, CancellationToken.None);
        purchases.Should().ContainSingle();
        purchases.First().PurchasedBy.Should().BeNull();
    }
}
