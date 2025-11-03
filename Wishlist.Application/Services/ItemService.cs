using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Wishlist.Application.Common;
using Wishlist.Application.Interfaces;
using Wishlist.Application.Models;
using Wishlist.Contracts.Common;
using Wishlist.Contracts.Items;
using Wishlist.Domain.Entities;
using Wishlist.Domain.Enums;
using System.Linq.Expressions;

namespace Wishlist.Application.Services;

public class ItemService : IItemService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly ILinkHealthService _linkHealthService;

    public ItemService(IAppDbContext dbContext, ICurrentUserService currentUser, IMapper mapper, ILinkHealthService linkHealthService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _mapper = mapper;
        _linkHealthService = linkHealthService;
    }

    public async Task<ItemDto> CreateAsync(ItemCreateRequest request, CancellationToken cancellationToken)
    {
        var list = await RequireListAsync(request.ListId, cancellationToken);
        EnsureCanEdit(list);

        var item = new Item
        {
            ListId = request.ListId,
            Title = request.Title,
            Note = request.Note,
            ProductUrl = request.ProductUrl,
            ImageUrl = request.ImageUrl,
            PriceAmount = request.PriceAmount,
            PriceCurrency = request.PriceCurrency,
            Retailer = request.Retailer,
            VariantColor = request.VariantColor,
            VariantSize = request.VariantSize,
            Quantity = request.Quantity,
            Priority = request.Priority,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _linkHealthService.MarkAsync(item.Id, cancellationToken);

        return _mapper.Map<ItemDto>(item);
    }

    public async Task<ItemDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _dbContext.Items.Include(i => i.List).FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (item is null)
        {
            throw new NotFoundException("item.notFound", "Item not found");
        }

        await EnsureReadAccessAsync(item.List, cancellationToken);
        return _mapper.Map<ItemDto>(item);
    }

    public async Task<PagedResult<ItemDto>> GetByListAsync(Guid listId, QueryParameters query, string? status, CancellationToken cancellationToken)
    {
        var list = await RequireListAsync(listId, cancellationToken);
        await EnsureReadAccessAsync(list, cancellationToken);

        var baseQuery = _dbContext.Items.Where(i => i.ListId == listId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            baseQuery = baseQuery.Where(i => EF.Functions.ILike(i.Title, $"%{query.Search!}%") || (i.Note != null && EF.Functions.ILike(i.Note, $"%{query.Search!}%")) || (i.Retailer != null && EF.Functions.ILike(i.Retailer, $"%{query.Search!}%")));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            status = status.ToLowerInvariant();
            baseQuery = status switch
            {
                "available" => baseQuery.Where(i => !_dbContext.Reservations.Any(r => r.ItemId == i.Id) && !_dbContext.Purchases.Any(p => p.ItemId == i.Id)),
                "reserved" => baseQuery.Where(i => _dbContext.Reservations.Any(r => r.ItemId == i.Id)),
                "purchased" => baseQuery.Where(i => _dbContext.Purchases.Any(p => p.ItemId == i.Id)),
                _ => baseQuery
            };
        }

        var total = await baseQuery.LongCountAsync(cancellationToken);

        IOrderedQueryable<Item>? ordered = null;
        if (!string.IsNullOrWhiteSpace(query.Sort))
        {
            foreach (var segment in query.Sort.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = segment.Split(':', StringSplitOptions.RemoveEmptyEntries);
                var field = parts[0];
                var direction = parts.Length > 1 && parts[1].Equals("asc", StringComparison.OrdinalIgnoreCase);

                ordered = (field.ToLowerInvariant()) switch
                {
                    "title" => ApplyOrder(ordered, baseQuery, i => i.Title, direction),
                    "createdat" => ApplyOrder(ordered, baseQuery, i => i.CreatedAt, direction),
                    _ => ordered
                };
            }
        }

        var queryable = ordered ?? baseQuery.OrderByDescending(i => i.CreatedAt);

        var items = await queryable
            .Skip((query.Pagination.Page - 1) * query.Pagination.PageSize)
            .Take(query.Pagination.PageSize)
            .ProjectTo<ItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<ItemDto>(items, query.Pagination.Page, query.Pagination.PageSize, total);
    }

    public async Task<ItemDto> UpdateAsync(Guid id, ItemUpdateRequest request, CancellationToken cancellationToken)
    {
        var item = await _dbContext.Items.Include(i => i.List).FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (item is null)
        {
            throw new NotFoundException("item.notFound", "Item not found");
        }

        EnsureCanEdit(item.List);

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            item.Title = request.Title!;
        }
        if (request.Note is not null)
        {
            item.Note = request.Note;
        }
        if (request.ProductUrl is not null)
        {
            item.ProductUrl = request.ProductUrl;
        }
        if (request.ImageUrl is not null)
        {
            item.ImageUrl = request.ImageUrl;
        }
        if (request.PriceAmount.HasValue)
        {
            item.PriceAmount = request.PriceAmount;
        }
        if (request.PriceCurrency is not null)
        {
            item.PriceCurrency = request.PriceCurrency;
        }
        if (request.Retailer is not null)
        {
            item.Retailer = request.Retailer;
        }
        if (request.VariantColor is not null)
        {
            item.VariantColor = request.VariantColor;
        }
        if (request.VariantSize is not null)
        {
            item.VariantSize = request.VariantSize;
        }
        if (request.Quantity.HasValue)
        {
            item.Quantity = request.Quantity.Value;
        }
        if (request.Priority.HasValue)
        {
            item.Priority = request.Priority.Value;
        }

        item.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (request.ProductUrl is not null)
        {
            await _linkHealthService.MarkAsync(item.Id, cancellationToken);
        }

        return _mapper.Map<ItemDto>(item);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _dbContext.Items.Include(i => i.List).FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (item is null)
        {
            throw new NotFoundException("item.notFound", "Item not found");
        }

        EnsureCanEdit(item.List);

        _dbContext.Items.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<List> RequireListAsync(Guid id, CancellationToken cancellationToken)
    {
        var list = await _dbContext.Lists.Include(l => l.Owner).Include(l => l.Members).FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (list is null)
        {
            throw new NotFoundException("list.notFound", "List not found");
        }

        return list;
    }

    private void EnsureCanEdit(List list)
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new ForbiddenException("item.forbidden", "Not allowed");
        }

        if (list.OwnerId == _currentUser.UserId)
        {
            return;
        }

        var member = list.Members.FirstOrDefault(m => m.UserId == _currentUser.UserId);
        if (member is null || (member.Role != ListRole.Owner && member.Role != ListRole.Editor))
        {
            throw new ForbiddenException("item.forbidden", "Not allowed");
        }
    }

    private async Task EnsureReadAccessAsync(List list, CancellationToken cancellationToken)
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

        if (list.Visibility == ListVisibility.Public)
        {
            return;
        }

        if (list.Visibility == ListVisibility.Link)
        {
            return;
        }

        throw new ForbiddenException("item.forbidden", "Not allowed");
    }

    private static IOrderedQueryable<Item> ApplyOrder<TKey>(IOrderedQueryable<Item>? ordered, IQueryable<Item> source, System.Linq.Expressions.Expression<Func<Item, TKey>> selector, bool ascending)
    {
        if (ordered is null)
        {
            return ascending ? source.OrderBy(selector) : source.OrderByDescending(selector);
        }

        return ascending ? ordered.ThenBy(selector) : ordered.ThenByDescending(selector);
    }
}
