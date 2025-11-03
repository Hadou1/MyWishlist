using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wishlist.Application.Security;
using Wishlist.Domain.Entities;
using Wishlist.Domain.Enums;

namespace Wishlist.Infrastructure.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DataSeeder");
        var context = scope.ServiceProvider.GetRequiredService<Persistence.AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        if (await context.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var demoUser = new User
        {
            Email = "demo@local",
            PasswordHash = hasher.HashPassword("Passw0rd!"),
            Name = "Demo User",
            Locale = "de-DE",
            Country = "DE",
            Currency = "EUR",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var secondUser = new User
        {
            Email = "friend@local",
            PasswordHash = hasher.HashPassword("Passw0rd!"),
            Name = "Friend",
            Locale = "de-DE",
            Country = "DE",
            Currency = "EUR",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(demoUser, secondUser);

        var list1 = new List
        {
            Owner = demoUser,
            Title = "Hochzeit",
            Occasion = "Wedding",
            EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(2)),
            Description = "Wunschliste zur Hochzeit",
            Visibility = ListVisibility.Private,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var list2 = new List
        {
            Owner = demoUser,
            Title = "Geburtstag",
            Occasion = "Birthday",
            EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
            Description = "Geburtstagsideen",
            Visibility = ListVisibility.Link,
            ShareSlug = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Lists.AddRange(list1, list2);

        context.ListMembers.AddRange(
            new ListMember { List = list1, User = demoUser, Role = ListRole.Owner, CreatedAt = DateTime.UtcNow },
            new ListMember { List = list2, User = demoUser, Role = ListRole.Owner, CreatedAt = DateTime.UtcNow },
            new ListMember { List = list2, User = secondUser, Role = ListRole.Viewer, CreatedAt = DateTime.UtcNow }
        );

        var items = new List<Item>
        {
            new Item { List = list1, Title = "Kaffeemaschine", Quantity = 1, Priority = ItemPriority.High, PriceAmount = 199.99m, PriceCurrency = "EUR", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Item { List = list1, Title = "Reisegutschein", Quantity = 1, Priority = ItemPriority.Medium, PriceAmount = 100m, PriceCurrency = "EUR", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Item { List = list1, Title = "Weingläser", Quantity = 6, Priority = ItemPriority.Low, PriceAmount = 59.99m, PriceCurrency = "EUR", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Item { List = list2, Title = "Laufschuhe", Quantity = 1, Priority = ItemPriority.High, PriceAmount = 120m, PriceCurrency = "EUR", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Item { List = list2, Title = "Kochbuch", Quantity = 1, Priority = ItemPriority.Medium, PriceAmount = 30m, PriceCurrency = "EUR", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Item { List = list2, Title = "Bluetooth Speaker", Quantity = 1, Priority = ItemPriority.Medium, PriceAmount = 80m, PriceCurrency = "EUR", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        context.Items.AddRange(items);

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seed data applied");
    }
}
