using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wishlist.Application.Interfaces;
using Wishlist.Application.Models;
using Wishlist.Contracts.Invites;
using Wishlist.Domain.Entities;
using Wishlist.Domain.Enums;

namespace Wishlist.Application.Services;

public class InviteService : IInviteService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public InviteService(IAppDbContext dbContext, ICurrentUserService currentUser, IMapper mapper)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<InviteDto> CreateAsync(Guid listId, InviteCreateRequest request, CancellationToken cancellationToken)
    {
        var list = await _dbContext.Lists.Include(l => l.Owner).Include(l => l.Members).FirstOrDefaultAsync(l => l.Id == listId, cancellationToken);
        if (list is null)
        {
            throw new NotFoundException("list.notFound", "List not found");
        }

        EnsureCanManageInvites(list);

        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", string.Empty).Replace("+", string.Empty).Replace("/", string.Empty);
        var invite = new Invite
        {
            ListId = listId,
            Email = request.Email,
            Role = request.Role,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(14),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Invites.Add(invite);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<InviteDto>(invite);
    }

    public async Task<InviteDto> AcceptAsync(InviteAcceptRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new ForbiddenException("invite.loginRequired", "Login required to accept invite");
        }

        var invite = await _dbContext.Invites.FirstOrDefaultAsync(i => i.Token == request.Token, cancellationToken);
        if (invite is null)
        {
            throw new NotFoundException("invite.notFound", "Invite not found");
        }

        if (invite.ExpiresAt < DateTime.UtcNow)
        {
            throw new ValidationAppException("invite.expired", "Invite expired", new Dictionary<string, string[]>
            {
                ["token"] = new[] { "Invite expired" }
            });
        }

        var existingMembership = await _dbContext.ListMembers.FirstOrDefaultAsync(m => m.ListId == invite.ListId && m.UserId == _currentUser.UserId, cancellationToken);
        if (existingMembership is null)
        {
            _dbContext.ListMembers.Add(new ListMember
            {
                ListId = invite.ListId,
                UserId = _currentUser.UserId.Value,
                Role = invite.Role,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existingMembership.Role = invite.Role;
        }

        invite.AcceptedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<InviteDto>(invite);
    }

    public async Task<InviteDto?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        var invite = await _dbContext.Invites.FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
        return invite is null ? null : _mapper.Map<InviteDto>(invite);
    }

    private void EnsureCanManageInvites(List list)
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new ForbiddenException("invite.forbidden", "Not allowed");
        }

        if (list.OwnerId == _currentUser.UserId)
        {
            return;
        }

        var member = list.Members.FirstOrDefault(m => m.UserId == _currentUser.UserId);
        if (member is null || (member.Role != ListRole.Owner && member.Role != ListRole.Editor))
        {
            throw new ForbiddenException("invite.forbidden", "Not allowed");
        }
    }
}
