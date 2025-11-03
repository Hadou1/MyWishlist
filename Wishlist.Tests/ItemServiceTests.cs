using FluentAssertions;
using Wishlist.Application.Services;
using Wishlist.Contracts.Items;
using Wishlist.Domain.Entities;
using Wishlist.Domain.Enums;
using Wishlist.Tests.Helpers;
using Xunit;

namespace Wishlist.Tests;

public class ItemServiceTests
{
    [Fact]
    public async Task OwnerCanCreateItem()
    {
        using var fixture = new TestFixture();
        var ownerId = Guid.NewGuid();
        var currentUser = new TestCurrentUserService { UserId = ownerId, IsAuthenticated = true };
        var stubLink = new StubLinkHealthService();
        var service = new ItemService(fixture.Context, currentUser, fixture.Mapper, stubLink);

        var list = new List { OwnerId = ownerId, Title = "Test", Visibility = ListVisibility.Private, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        fixture.Context.Lists.Add(list);
        fixture.Context.ListMembers.Add(new ListMember { List = list, UserId = ownerId, Role = ListRole.Owner, CreatedAt = DateTime.UtcNow });
        await fixture.Context.SaveChangesAsync();

        var item = await service.CreateAsync(new ItemCreateRequest(list.Id, "Item", null, null, null, null, null, null, null, null, 1, ItemPriority.Medium), CancellationToken.None);
        item.Title.Should().Be("Item");
        stubLink.MarkedItems.Should().Contain(item.Id);
    }

    [Fact]
    public async Task ViewerCannotCreateItem()
    {
        using var fixture = new TestFixture();
        var ownerId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var currentUser = new TestCurrentUserService { UserId = viewerId, IsAuthenticated = true };
        var stubLink = new StubLinkHealthService();
        var service = new ItemService(fixture.Context, currentUser, fixture.Mapper, stubLink);

        var list = new List { OwnerId = ownerId, Title = "Test", Visibility = ListVisibility.Private, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        fixture.Context.Lists.Add(list);
        fixture.Context.ListMembers.Add(new ListMember { List = list, UserId = ownerId, Role = ListRole.Owner, CreatedAt = DateTime.UtcNow });
        fixture.Context.ListMembers.Add(new ListMember { List = list, UserId = viewerId, Role = ListRole.Viewer, CreatedAt = DateTime.UtcNow });
        await fixture.Context.SaveChangesAsync();

        var act = async () => await service.CreateAsync(new ItemCreateRequest(list.Id, "Item", null, null, null, null, null, null, null, null, 1, ItemPriority.Medium), CancellationToken.None);
        await act.Should().ThrowAsync<Wishlist.Application.Models.ForbiddenException>();
    }
}
