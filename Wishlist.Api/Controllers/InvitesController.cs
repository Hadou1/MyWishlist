using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wishlist.Application.Interfaces;
using Wishlist.Contracts.Invites;

namespace Wishlist.Api.Controllers;

[ApiController]
[Route("api/v1/invites")]
public class InvitesController : ControllerBase
{
    private readonly IInviteService _inviteService;

    public InvitesController(IInviteService inviteService)
    {
        _inviteService = inviteService;
    }

    [HttpPost("lists/{listId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(InviteDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(Guid listId, [FromBody] InviteCreateRequest request, CancellationToken cancellationToken)
    {
        var invite = await _inviteService.CreateAsync(listId, request, cancellationToken);
        return CreatedAtAction(nameof(GetByToken), new { token = invite.Token }, invite);
    }

    [HttpGet("{token}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(InviteDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByToken(string token, CancellationToken cancellationToken)
    {
        var invite = await _inviteService.GetByTokenAsync(token, cancellationToken);
        if (invite is null)
        {
            return NotFound();
        }

        return Ok(invite);
    }

    [HttpPost("accept")]
    [Authorize]
    [ProducesResponseType(typeof(InviteDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Accept([FromBody] InviteAcceptRequest request, CancellationToken cancellationToken)
    {
        var invite = await _inviteService.AcceptAsync(request, cancellationToken);
        return Ok(invite);
    }
}
