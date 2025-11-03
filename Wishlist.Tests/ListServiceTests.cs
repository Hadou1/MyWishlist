using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Wishlist.Application.Security;
using Wishlist.Application.Services;
using Wishlist.Contracts.Members;
using Wishlist.Domain.Entities;
using Wishlist.Domain.Enums;
using Wishlist.Tests.Helpers;
using Xunit;

namespace Wishlist.Tests;

public class ListServiceTests
{
    [Fact]
    public async Task OwnerCanAddMember()
    {
        using var fixture = new TestFixture();
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        fixture.Context.Users.Add(new User { Id = ownerId, Email = "owner@local", PasswordHash = "hash", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        fixture.Context.Users.Add(new User { Id = memberId, Email = "member@local", PasswordHash = "hash", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        var currentUser = new TestCurrentUserService { UserId = ownerId, IsAuthenticated = true };
        var service = new ListService(fixture.Context, currentUser, new Pbkdf2PasswordHasher(), fixture.Mapper, NullLogger<ListService>.Instance);
        var list = new List { OwnerId = ownerId, Title = "List", Visibility = ListVisibility.Private, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        fixture.Context.Lists.Add(list);
        fixture.Context.ListMembers.Add(new ListMember { List = list, UserId = ownerId, Role = ListRole.Owner, CreatedAt = DateTime.UtcNow });
        await fixture.Context.SaveChangesAsync();

        var member = await service.AddMemberAsync(list.Id, new ListMemberCreateRequest(memberId, ListRole.Viewer), CancellationToken.None);
        member.UserId.Should().Be(memberId);
    }

    [Fact]
    public async Task GenerateShareLink_ShouldSetSlug()
    {
        using var fixture = new TestFixture();
        var ownerId = Guid.NewGuid();
        var currentUser = new TestCurrentUserService { UserId = ownerId, IsAuthenticated = true };
        var service = new ListService(fixture.Context, currentUser, new Pbkdf2PasswordHasher(), fixture.Mapper, NullLogger<ListService>.Instance);
        var list = new List { OwnerId = ownerId, Title = "List", Visibility = ListVisibility.Private, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        fixture.Context.Lists.Add(list);
        fixture.Context.ListMembers.Add(new ListMember { List = list, UserId = ownerId, Role = ListRole.Owner, CreatedAt = DateTime.UtcNow });
        await fixture.Context.SaveChangesAsync();

        var link = await service.GenerateShareLinkAsync(list.Id, CancellationToken.None);
        link.ShareSlug.Should().NotBeNullOrWhiteSpace();
    }
}
