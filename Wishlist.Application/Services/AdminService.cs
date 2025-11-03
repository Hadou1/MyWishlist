using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Wishlist.Application.Common;
using Wishlist.Application.Interfaces;
using Wishlist.Application.Models;
using Wishlist.Contracts.Admin;
using Wishlist.Contracts.Common;
using Wishlist.Domain.Entities;

namespace Wishlist.Application.Services;

public class AdminService : IAdminService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public AdminService(IAppDbContext dbContext, ICurrentUserService currentUser, IMapper mapper)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(QueryParameters query, CancellationToken cancellationToken)
    {
        RequireAdmin();

        var baseQuery = _dbContext.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            baseQuery = baseQuery.Where(u => u.Email.Contains(query.Search!) || (u.Name != null && u.Name.Contains(query.Search!)));
        }

        var total = await baseQuery.LongCountAsync(cancellationToken);
        var users = await baseQuery
            .OrderByDescending(u => u.CreatedAt)
            .Skip((query.Pagination.Page - 1) * query.Pagination.PageSize)
            .Take(query.Pagination.PageSize)
            .ProjectTo<AdminUserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminUserDto>(users, query.Pagination.Page, query.Pagination.PageSize, total);
    }

    public async Task<PagedResult<AdminListDto>> GetListsAsync(QueryParameters query, CancellationToken cancellationToken)
    {
        RequireAdmin();

        var baseQuery = _dbContext.Lists.Include(l => l.Owner).AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            baseQuery = baseQuery.Where(l => l.Title.Contains(query.Search!) || (l.Description != null && l.Description.Contains(query.Search!)));
        }

        var total = await baseQuery.LongCountAsync(cancellationToken);
        var lists = await baseQuery
            .OrderByDescending(l => l.CreatedAt)
            .Skip((query.Pagination.Page - 1) * query.Pagination.PageSize)
            .Take(query.Pagination.PageSize)
            .ProjectTo<AdminListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminListDto>(lists, query.Pagination.Page, query.Pagination.PageSize, total);
    }

    public async Task ReportAsync(AbuseReportRequest request, CancellationToken cancellationToken)
    {
        RequireAdmin();

        var report = new AbuseReport
        {
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Action = request.Action,
            ActorId = _currentUser.UserId,
            ActorEmail = _currentUser.Email,
            Message = request.Message,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.AbuseReports.Add(report);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ModerateAsync(ModerationRequest request, CancellationToken cancellationToken)
    {
        RequireAdmin();

        // Basic moderation: soft delete list
        if (request.EntityType.Equals("list", StringComparison.OrdinalIgnoreCase))
        {
            var list = await _dbContext.Lists.FirstOrDefaultAsync(l => l.Id == request.EntityId, cancellationToken);
            if (list is not null)
            {
                list.DeletedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private void RequireAdmin()
    {
        if (!_currentUser.IsAdmin)
        {
            throw new ForbiddenException("admin.forbidden", "Admin rights required");
        }
    }
}
