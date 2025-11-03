using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wishlist.Application.Common;
using Wishlist.Application.Interfaces;
using Wishlist.Api.Models;
using Wishlist.Contracts.Common;
using Wishlist.Contracts.Lists;
using Wishlist.Contracts.Members;

namespace Wishlist.Api.Controllers;

[ApiController]
[Route("api/v1/lists")]
public class ListsController : ControllerBase
{
    private readonly IListService _listService;

    public ListsController(IListService listService)
    {
        _listService = listService;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ListSummary), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] ListCreateRequest request, CancellationToken cancellationToken)
    {
        var list = await _listService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = list.Id }, list);
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<ListSummary>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery(Name = "sort")] string? sort, [FromQuery(Name = "q")] string? search, [FromQuery] string? visibility, [FromQuery] string? occasion, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken)
    {
        var query = new QueryParameters(page, pageSize, sort, search);
        var result = await _listService.GetAsync(query, visibility, occasion, from, to, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ListDetail), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] string? passcode, CancellationToken cancellationToken)
    {
        var list = await _listService.GetByIdAsync(id, passcode, cancellationToken);
        if (list.List.Visibility == Wishlist.Domain.Enums.ListVisibility.Link)
        {
            Response.Headers["X-Robots-Tag"] = "noindex";
        }
        return Ok(list);
    }

    [HttpPatch("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ListSummary), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ListUpdateRequest request, CancellationToken cancellationToken)
    {
        var list = await _listService.UpdateAsync(id, request, cancellationToken);
        return Ok(list);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _listService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/members")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ListMemberDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembers(Guid id, CancellationToken cancellationToken)
    {
        var members = await _listService.GetMembersAsync(id, cancellationToken);
        return Ok(members);
    }

    [HttpPost("{id:guid}/members")]
    [Authorize]
    [ProducesResponseType(typeof(ListMemberDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] ListMemberCreateRequest request, CancellationToken cancellationToken)
    {
        var member = await _listService.AddMemberAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetMembers), new { id }, member);
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await _listService.RemoveMemberAsync(id, userId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/share-link")]
    [Authorize]
    [ProducesResponseType(typeof(ShareLinkResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateShareLink(Guid id, CancellationToken cancellationToken)
    {
        var link = await _listService.GenerateShareLinkAsync(id, cancellationToken);
        return Ok(link);
    }

    [HttpPost("{id:guid}/share-passcode")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetPasscode(Guid id, [FromBody] PasscodeRequest request, CancellationToken cancellationToken)
    {
        await _listService.SetPasscodeAsync(id, request.Passcode, cancellationToken);
        return NoContent();
    }
}
