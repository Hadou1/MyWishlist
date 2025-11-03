using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wishlist.Application.Interfaces;
using Wishlist.Application.Models;
using Wishlist.Contracts.Reservations;
using Wishlist.Domain.Entities;
using Wishlist.Domain.Enums;

namespace Wishlist.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public ReservationService(IAppDbContext dbContext, ICurrentUserService currentUser, IMapper mapper)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<ReservationDto> CreateAsync(Guid itemId, ReservationCreateRequest request, CancellationToken cancellationToken)
    {
        var item = await _dbContext.Items.Include(i => i.List).ThenInclude(l => l.Members).FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
        if (item is null)
        {
            throw new NotFoundException("item.notFound", "Item not found");
        }

        await EnsureReservationAllowedAsync(item.List, cancellationToken);

        var existing = await _dbContext.Reservations.FirstOrDefaultAsync(r => r.ItemId == itemId, cancellationToken);
        if (existing is not null)
        {
            throw new ValidationAppException("reservation.exists", "Item already reserved", new Dictionary<string, string[]>
            {
                ["itemId"] = new[] { "Item already reserved" }
            });
        }

        var reservation = new Reservation
        {
            ItemId = itemId,
            ReservedByUserId = _currentUser.UserId,
            ReservedByEmail = _currentUser.IsAuthenticated ? _currentUser.Email : request.Email,
            Message = request.Message,
            IsVisibleToOwner = request.IsVisibleToOwner ?? false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (!_currentUser.IsAuthenticated && string.IsNullOrWhiteSpace(reservation.ReservedByEmail))
        {
            throw new ValidationAppException("reservation.emailRequired", "Email required for guest reservations", new Dictionary<string, string[]>
            {
                ["email"] = new[] { "Email required for guest reservations" }
            });
        }

        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapReservation(item.List.OwnerId, reservation);
    }

    public async Task<IReadOnlyCollection<ReservationDto>> GetForItemAsync(Guid itemId, CancellationToken cancellationToken)
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

        var reservations = await _dbContext.Reservations.Where(r => r.ItemId == itemId).ToListAsync(cancellationToken);
        return reservations.Select(r => MapReservation(item.List.OwnerId, r)).ToList();
    }

    public async Task DeleteAsync(Guid reservationId, CancellationToken cancellationToken)
    {
        var reservation = await _dbContext.Reservations.Include(r => r.Item).ThenInclude(i => i.List).FirstOrDefaultAsync(r => r.Id == reservationId, cancellationToken);
        if (reservation is null)
        {
            throw new NotFoundException("reservation.notFound", "Reservation not found");
        }

        var list = reservation.Item.List;
        if (_currentUser.IsAdmin || (_currentUser.UserId.HasValue && reservation.ReservedByUserId == _currentUser.UserId))
        {
            _dbContext.Reservations.Remove(reservation);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        if (list.OwnerId == _currentUser.UserId)
        {
            _dbContext.Reservations.Remove(reservation);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        throw new ForbiddenException("reservation.forbidden", "Not allowed");
    }

    private async Task EnsureReservationAllowedAsync(List list, CancellationToken cancellationToken)
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

        if (list.Visibility == ListVisibility.Link)
        {
            return;
        }

        if (list.Visibility == ListVisibility.Public)
        {
            return;
        }

        throw new ForbiddenException("reservation.forbidden", "Not allowed");
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

        throw new ForbiddenException("reservation.forbidden", "Not allowed");
    }

    private ReservationDto MapReservation(Guid ownerId, Reservation reservation)
    {
        var isOwner = _currentUser.UserId.HasValue && _currentUser.UserId == ownerId;
        var showIdentity = reservation.IsVisibleToOwner || (_currentUser.UserId.HasValue && reservation.ReservedByUserId == _currentUser.UserId) || _currentUser.IsAdmin;
        var display = showIdentity ? reservation.ReservedByEmail ?? reservation.ReservedByUserId?.ToString() : null;
        if (isOwner && !reservation.IsVisibleToOwner)
        {
            display = null;
        }

        return new ReservationDto(reservation.Id, reservation.ItemId, display, reservation.Message, reservation.IsVisibleToOwner, reservation.CreatedAt);
    }
}
