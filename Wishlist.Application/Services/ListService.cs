using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wishlist.Application.Common;
using Wishlist.Application.Interfaces;
using Wishlist.Application.Models;
using Wishlist.Application.Security;
using Wishlist.Contracts.Common;
using Wishlist.Contracts.Lists;
using Wishlist.Contracts.Members;
using Wishlist.Domain.Entities;
using Wishlist.Domain.Enums;
using System.Linq.Expressions;

namespace Wishlist.Application.Services;

public class ListService : IListService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IPasswordHasher _passcodeHasher;
    private readonly IMapper _mapper;
    private readonly ILogger<ListService> _logger;

    public ListService(IAppDbContext dbContext, ICurrentUserService currentUser, IPasswordHasher passcodeHasher, IMapper mapper, ILogger<ListService> logger)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _passcodeHasher = passcodeHasher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ListSummary> CreateAsync(ListCreateRequest request, CancellationToken cancellationToken)
    {
        RequireAuthentication();

        var list = new List
        {
            OwnerId = _currentUser.UserId!.Value,
            Title = request.Title,
            Occasion = request.Occasion,
            EventDate = request.EventDate,
            Description = request.Description,
            CoverImageUrl = request.CoverImageUrl,
            Visibility = ListVisibility.Private,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Lists.Add(list);
        _dbContext.ListMembers.Add(new ListMember
        {
            List = list,
            UserId = list.OwnerId,
            Role = ListRole.Owner,
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ListSummary>(list);
    }

    public async Task<PagedResult<ListSummary>> GetAsync(QueryParameters query, string? visibility, string? occasion, DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        RequireAuthentication();

        var userId = _currentUser.UserId!.Value;
        var baseQuery = _dbContext.Lists
            .Include(l => l.Owner)
            .Where(l => l.OwnerId == userId || l.Members.Any(m => m.UserId == userId));

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            baseQuery = baseQuery.Where(l => EF.Functions.ILike(l.Title, $"%{query.Search!}%") || (l.Description != null && EF.Functions.ILike(l.Description, $"%{query.Search!}%")));
        }

        if (!string.IsNullOrWhiteSpace(visibility) && Enum.TryParse<ListVisibility>(visibility, true, out var visibilityFilter))
        {
            baseQuery = baseQuery.Where(l => l.Visibility == visibilityFilter);
        }

        if (!string.IsNullOrWhiteSpace(occasion))
        {
            baseQuery = baseQuery.Where(l => l.Occasion != null && EF.Functions.ILike(l.Occasion, $"%{occasion}%"));
        }

        if (from.HasValue)
        {
            baseQuery = baseQuery.Where(l => l.EventDate >= from);
        }

        if (to.HasValue)
        {
            baseQuery = baseQuery.Where(l => l.EventDate <= to);
        }

        var total = await baseQuery.LongCountAsync(cancellationToken);

        IOrderedQueryable<List>? ordered = null;
        if (!string.IsNullOrWhiteSpace(query.Sort))
        {
            foreach (var segment in query.Sort.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = segment.Split(':', StringSplitOptions.RemoveEmptyEntries);
                var field = parts[0].ToLowerInvariant();
                var ascending = parts.Length > 1 && parts[1].Equals("asc", StringComparison.OrdinalIgnoreCase);

                ordered = field switch
                {
                    "title" => ApplyOrder(ordered, baseQuery, l => l.Title, ascending),
                    "createdat" => ApplyOrder(ordered, baseQuery, l => l.CreatedAt, ascending),
                    _ => ordered
                };
            }
        }

        var queryable = ordered ?? baseQuery.OrderByDescending(l => l.CreatedAt);

        var items = await queryable
            .Skip((query.Pagination.Page - 1) * query.Pagination.PageSize)
            .Take(query.Pagination.PageSize)
            .ProjectTo<ListSummary>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<ListSummary>(items, query.Pagination.Page, query.Pagination.PageSize, total);
    }

    public async Task<ListDetail> GetByIdAsync(Guid id, string? passcode, CancellationToken cancellationToken)
    {
        var list = await _dbContext.Lists
            .Include(l => l.Owner)
            .Include(l => l.Members).ThenInclude(m => m.User)
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (list is null)
        {
            throw new NotFoundException("list.notFound", "List not found");
        }

        var hasAccess = await HasListAccessAsync(list, passcode, cancellationToken);
        if (!hasAccess)
        {
            throw new ForbiddenException("list.forbidden", "Not allowed to access the list");
        }

        var items = list.Items.Select(i => _mapper.Map<Contracts.Items.ItemDto>(i)).ToList();
        var members = list.Members.Select(m => _mapper.Map<ListMemberDto>(m)).ToList();

        return new ListDetail(_mapper.Map<ListSummary>(list), members, items);
    }

    public async Task<ListSummary> UpdateAsync(Guid id, ListUpdateRequest request, CancellationToken cancellationToken)
    {
        RequireAuthentication();
        var list = await RequireListAsync(id, cancellationToken);
        EnsureOwner(list);

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            list.Title = request.Title!;
        }
        if (!string.IsNullOrWhiteSpace(request.Occasion))
        {
            list.Occasion = request.Occasion;
        }
        if (request.EventDate.HasValue)
        {
            list.EventDate = request.EventDate;
        }
        if (request.Description is not null)
        {
            list.Description = request.Description;
        }
        if (!string.IsNullOrWhiteSpace(request.CoverImageUrl))
        {
            list.CoverImageUrl = request.CoverImageUrl;
        }
        if (request.Visibility.HasValue)
        {
            list.Visibility = request.Visibility.Value;
        }
        if (request.RemovePasscode == true)
        {
            list.PasscodeHash = null;
        }
        else if (!string.IsNullOrWhiteSpace(request.Passcode))
        {
            list.PasscodeHash = _passcodeHasher.HashPassword(request.Passcode);
        }

        list.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ListSummary>(list);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        RequireAuthentication();
        var list = await RequireListAsync(id, cancellationToken);
        EnsureOwner(list);

        _dbContext.Lists.Remove(list);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("List {ListId} deleted by {UserId}", id, _currentUser.UserId);
    }

    public async Task<IReadOnlyCollection<ListMemberDto>> GetMembersAsync(Guid listId, CancellationToken cancellationToken)
    {
        var list = await RequireListAsync(listId, cancellationToken);
        EnsureAtLeastRole(list, ListRole.Viewer);

        return await _dbContext.ListMembers
            .Where(m => m.ListId == listId)
            .Include(m => m.User)
            .ProjectTo<ListMemberDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<ListMemberDto> AddMemberAsync(Guid listId, ListMemberCreateRequest request, CancellationToken cancellationToken)
    {
        RequireAuthentication();
        var list = await RequireListAsync(listId, cancellationToken);
        EnsureOwner(list);

        if (!await _dbContext.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken))
        {
            throw new NotFoundException("user.notFound", "User not found");
        }

        if (await _dbContext.ListMembers.AnyAsync(m => m.ListId == listId && m.UserId == request.UserId, cancellationToken))
        {
            throw new ValidationAppException("member.exists", "Member already assigned", new Dictionary<string, string[]>
            {
                ["userId"] = new[] { "User already part of the list" }
            });
        }

        var member = new ListMember
        {
            ListId = listId,
            UserId = request.UserId,
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ListMembers.Add(member);
        await _dbContext.SaveChangesAsync(cancellationToken);

        member = await _dbContext.ListMembers.Include(m => m.User).FirstAsync(m => m.ListId == listId && m.UserId == request.UserId, cancellationToken);
        return _mapper.Map<ListMemberDto>(member);
    }

    public async Task RemoveMemberAsync(Guid listId, Guid userId, CancellationToken cancellationToken)
    {
        RequireAuthentication();
        var list = await RequireListAsync(listId, cancellationToken);
        EnsureOwner(list);

        var member = await _dbContext.ListMembers.FirstOrDefaultAsync(m => m.ListId == listId && m.UserId == userId, cancellationToken);
        if (member is null)
        {
            throw new NotFoundException("member.notFound", "Member not found");
        }

        if (member.Role == ListRole.Owner)
        {
            throw new ValidationAppException("member.owner", "Owner cannot be removed", new Dictionary<string, string[]>
            {
                ["userId"] = new[] { "Owner cannot be removed" }
            });
        }

        _dbContext.ListMembers.Remove(member);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ShareLinkResponse> GenerateShareLinkAsync(Guid listId, CancellationToken cancellationToken)
    {
        RequireAuthentication();
        var list = await RequireListAsync(listId, cancellationToken);
        EnsureOwner(list);

        list.ShareSlug = list.ShareSlug ?? $"{Guid.NewGuid():N}";
        list.Visibility = ListVisibility.Link;
        list.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ShareLinkResponse(list.ShareSlug!, $"/lists/share/{list.ShareSlug}");
    }

    public async Task SetPasscodeAsync(Guid listId, string? passcode, CancellationToken cancellationToken)
    {
        RequireAuthentication();
        var list = await RequireListAsync(listId, cancellationToken);
        EnsureOwner(list);

        list.PasscodeHash = string.IsNullOrWhiteSpace(passcode) ? null : _passcodeHasher.HashPassword(passcode);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void RequireAuthentication()
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new ForbiddenException("user.notAuthenticated", "User must be authenticated");
        }
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

    private void EnsureOwner(List list)
    {
        if (!_currentUser.UserId.HasValue || list.OwnerId != _currentUser.UserId)
        {
            throw new ForbiddenException("list.notOwner", "Only the owner can perform this action");
        }
    }

    private void EnsureAtLeastRole(List list, ListRole role)
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new ForbiddenException("list.forbidden", "Not allowed");
        }

        if (list.OwnerId == _currentUser.UserId)
        {
            return;
        }

        var membership = list.Members.FirstOrDefault(m => m.UserId == _currentUser.UserId);
        if (membership is null || membership.Role > role)
        {
            throw new ForbiddenException("list.forbidden", "Not allowed");
        }
    }

    private async Task<bool> HasListAccessAsync(List list, string? passcode, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId.HasValue)
        {
            if (list.OwnerId == _currentUser.UserId)
            {
                return true;
            }

            var membership = await _dbContext.ListMembers.FirstOrDefaultAsync(m => m.ListId == list.Id && m.UserId == _currentUser.UserId, cancellationToken);
            if (membership is not null)
            {
                return true;
            }
        }

        if (list.Visibility == ListVisibility.Public)
        {
            return true;
        }

        if (list.Visibility == ListVisibility.Link)
        {
            if (string.IsNullOrWhiteSpace(list.PasscodeHash))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(passcode) && _passcodeHasher.VerifyHashedPassword(list.PasscodeHash, passcode))
            {
                return true;
            }
        }

        return false;
    }

    private static IOrderedQueryable<List> ApplyOrder<TKey>(IOrderedQueryable<List>? ordered, IQueryable<List> source, System.Linq.Expressions.Expression<Func<List, TKey>> selector, bool ascending)
    {
        if (ordered is null)
        {
            return ascending ? source.OrderBy(selector) : source.OrderByDescending(selector);
        }

        return ascending ? ordered.ThenBy(selector) : ordered.ThenByDescending(selector);
    }
}
