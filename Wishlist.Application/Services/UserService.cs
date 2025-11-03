using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wishlist.Application.Interfaces;
using Wishlist.Application.Models;
using Wishlist.Contracts.Gdpr;
using Wishlist.Contracts.Users;

namespace Wishlist.Application.Services;

public class UserService : IUserService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(IAppDbContext dbContext, ICurrentUserService currentUser, IMapper mapper, ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var user = await RequireCurrentUserAsync(cancellationToken);
        return _mapper.Map<UserDto>(user);
    }

    public async Task DeleteCurrentUserAsync(CancellationToken cancellationToken)
    {
        var user = await RequireCurrentUserAsync(cancellationToken);

        user.Email = $"deleted-{user.Id}@local";
        user.Name = null;
        user.Locale = null;
        user.Country = null;
        user.Currency = null;
        user.PasswordHash = string.Empty;
        user.DeletedAt = DateTime.UtcNow;

        var reservations = await _dbContext.Reservations.Where(r => r.ReservedByUserId == user.Id).ToListAsync(cancellationToken);
        foreach (var reservation in reservations)
        {
            reservation.ReservedByUserId = null;
            reservation.ReservedByEmail = null;
        }

        var purchases = await _dbContext.Purchases.Where(p => p.PurchasedByUserId == user.Id).ToListAsync(cancellationToken);
        foreach (var purchase in purchases)
        {
            purchase.PurchasedByUserId = null;
            purchase.PurchasedByEmail = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User {UserId} anonymised", user.Id);
    }

    public async Task<GdprExportResponse> ExportAsync(CancellationToken cancellationToken)
    {
        var user = await RequireCurrentUserAsync(cancellationToken);

        var lists = await _dbContext.Lists
            .Include(l => l.Items)
            .Include(l => l.Invites)
            .Include(l => l.Owner)
            .Include(l => l.Members)
            .Where(l => l.OwnerId == user.Id || l.Members.Any(m => m.UserId == user.Id))
            .ToListAsync(cancellationToken);

        var itemIds = lists.SelectMany(l => l.Items.Select(i => i.Id)).ToList();

        var reservations = await _dbContext.Reservations
            .Include(r => r.Item)
            .Where(r => itemIds.Contains(r.ItemId))
            .ToListAsync(cancellationToken);
        var purchases = await _dbContext.Purchases
            .Include(p => p.Item)
            .Where(p => itemIds.Contains(p.ItemId))
            .ToListAsync(cancellationToken);

        var listSnapshots = lists.Select(l =>
        {
            var lReservations = reservations.Where(r => r.Item.ListId == l.Id).ToList();
            var lPurchases = purchases.Where(p => p.Item.ListId == l.Id).ToList();
            return new ListSnapshot(
                _mapper.Map<ListSummary>(l),
                l.Items.Select(i => new ItemSnapshot(i.Id, i.ListId, i.Title, i.Note, i.ProductUrl, i.ImageUrl, i.PriceAmount, i.PriceCurrency, i.Retailer, i.VariantColor, i.VariantSize, i.Quantity, (int)i.Priority, (int)i.LinkStatus, i.CreatedAt)).ToList(),
                lReservations.Select(r => new ReservationSnapshot(r.Id, r.ItemId, r.IsVisibleToOwner ? r.ReservedByUserId : null, r.IsVisibleToOwner ? r.ReservedByEmail : null, r.Message, r.IsVisibleToOwner, r.CreatedAt)).ToList(),
                lPurchases.Select(p => new PurchaseSnapshot(p.Id, p.ItemId, p.IsVisibleToOwner ? p.PurchasedByUserId : null, p.IsVisibleToOwner ? p.PurchasedByEmail : null, p.PaidPriceAmount, p.ReceiptUrl, p.IsVisibleToOwner, p.CreatedAt)).ToList()
            );
        }).ToList();

        return new GdprExportResponse(
            _mapper.Map<UserSnapshot>(user),
            listSnapshots
        );
    }

    private async Task<Domain.Entities.User> RequireCurrentUserAsync(CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new ForbiddenException("user.notAuthenticated", "User is not authenticated");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("user.notFound", "User not found");
        }

        return user;
    }
}
