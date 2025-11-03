using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wishlist.Application.Interfaces;
using Wishlist.Application.Security;
using Wishlist.Application.Services;
using Wishlist.Infrastructure.Identity;
using Wishlist.Infrastructure.Persistence;
using Wishlist.Infrastructure.Services;

namespace Wishlist.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") ?? configuration["DB_CONNECTION"] ?? "Host=localhost;Port=5432;Database=wishlist;Username=postgres;Password=postgres";

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, Wishlist.Application.Security.Pbkdf2PasswordHasher>();
        services.AddScoped<ILinkHealthService, FakeLinkHealthService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IListService, ListService>();
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<IInviteService, InviteService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}
