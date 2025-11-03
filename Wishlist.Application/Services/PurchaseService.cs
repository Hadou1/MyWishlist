using Microsoft.EntityFrameworkCore;
using Wishlist.Application.Interfaces;
using Wishlist.Application.Models;
using Wishlist.Contracts.Purchases;
using Wishlist.Domain.Entities;
using Wishlist.Domain.Enums;

namespace Wishlist.Application.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public PurchaseService(IAppDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<PurchaseDto> CreateAsync(Guid itemId, PurchaseCreateRequest request, CancellationToken cancellationToken)
    {
        var item = await _dbContext.Items.Include(i => i.List).ThenInclude(l => l.Members).FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
        if (item is null)
        {
            throw new NotFoundException("item.notFound", "Item not found");
        }

        await EnsurePurchaseAllowedAsync(item.List, cancellationToken);

        if (await _dbContext.Purchases.AnyAsync(p => p.ItemId == itemId, cancellationToken))
        {
            throw new ValidationAppException("purchase.exists", "Item already purchased", new Dictionary<string, string[]>
            {
                ["itemId"] = new[] { "Item already purchased" }
            });
        }

        var purchase = new Purchase
        {
            ItemId = itemId,
            PurchasedByUserId = _currentUser.UserId,
            PurchasedByEmail = _currentUser.IsAuthenticated ? _currentUser.Email : request.Email,
            PaidPriceAmount = request.PaidPriceAmount,
            ReceiptUrl = request.ReceiptUrl,
            IsVisibleToOwner = request.IsVisibleToOwner ?? false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (!_currentUser.IsAuthenticated && string.IsNullOrWhiteSpace(purchase.PurchasedByEmail))
        {
            throw new ValidationAppException("purchase.emailRequired", "Email required for guest purchases", new Dictionary<string, string[]>
            {
                ["email"] = new[] { "Email required for guest purchases" }
            });
        }

        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapPurchase(item.List.OwnerId, purchase);
    }

    public async Task<IReadOnlyCollection<PurchaseDto>> GetForItemAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var item = await _dbContext.Items
            .Include(i => i.List).ThenInclude(l => l.Members)
            .Include(i => i.List).ThenInclude(l => l.Owner)
            .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
        if (item is null)
        {
            throw new NotFoundException("item.notFound", "Item not found");
        }

        await EnsureReadAllowedAsync(item.List, cancellationToken);

        var purchases = await _dbContext.Purchases.Where(p => p.ItemId == itemId).ToListAsync(cancellationToken);
        return purchases.Select(p => MapPurchase(item.List.OwnerId, p)).ToList();
    }

    private async Task EnsurePurchaseAllowedAsync(List list, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId.HasValue)
        {
            if (list.OwnerId == _currentUser.UserId)
            {
                return;
            }

            var membership = await _dbContext.ListMembers.FirstOrDefaultAsync(m => m.ListId == list.Id && m.UserId == _currentUser.UserId, cancellationToken);
            if (membership is not null)
            {
                return;
            }
        }

        if (list.Visibility == ListVisibility.Link || list.Visibility == ListVisibility.Public)
        {
            return;
        }

        throw new ForbiddenException("purchase.forbidden", "Not allowed");
    }

    private async Task EnsureReadAllowedAsync(List list, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId.HasValue)
        {
            if (list.OwnerId == _currentUser.UserId)
            {
                return;
            }

            var membership = await _dbContext.ListMembers.FirstOrDefaultAsync(m => m.ListId == list.Id && m.UserId == _currentUser.UserId, cancellationToken);
            if (membership is not null)
            {
                return;
            }
        }

        if (list.Visibility != ListVisibility.Private)
        {
            return;
        }

        throw new ForbiddenException("purchase.forbidden", "Not allowed");
    }

    private PurchaseDto MapPurchase(Guid ownerId, Purchase purchase)
    {
        var isOwner = _currentUser.UserId.HasValue && _currentUser.UserId == ownerId;
        var showIdentity = purchase.IsVisibleToOwner || (_currentUser.UserId.HasValue && purchase.PurchasedByUserId == _currentUser.UserId) || _currentUser.IsAdmin;
        var display = showIdentity ? purchase.PurchasedByEmail ?? purchase.PurchasedByUserId?.ToString() : null;
        if (isOwner && !purchase.IsVisibleToOwner)
        {
            display = null;
        }

        return new PurchaseDto(purchase.Id, purchase.ItemId, display, purchase.PaidPriceAmount, purchase.ReceiptUrl, purchase.IsVisibleToOwner, purchase.CreatedAt);
    }
}
