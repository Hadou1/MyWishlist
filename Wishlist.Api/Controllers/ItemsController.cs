using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wishlist.Application.Common;
using Wishlist.Application.Interfaces;
using Wishlist.Contracts.Common;
using Wishlist.Contracts.Items;

namespace Wishlist.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;

    public ItemsController(IItemService itemService)
    {
        _itemService = itemService;
    }

    [HttpPost("items")]
    [Authorize]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] ItemCreateRequest request, CancellationToken cancellationToken)
    {
        var item = await _itemService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { itemId = item.Id }, item);
    }

    [HttpGet("lists/{listId:guid}/items")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<ItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByList(Guid listId, [FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] string? sort, [FromQuery(Name = "q")] string? search, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        var query = new QueryParameters(page, pageSize, sort, search);
        var items = await _itemService.GetByListAsync(listId, query, status, cancellationToken);
        return Ok(items);
    }

    [HttpGet("items/{itemId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid itemId, CancellationToken cancellationToken)
    {
        var item = await _itemService.GetAsync(itemId, cancellationToken);
        return Ok(item);
    }

    [HttpPatch("items/{itemId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid itemId, [FromBody] ItemUpdateRequest request, CancellationToken cancellationToken)
    {
        var item = await _itemService.UpdateAsync(itemId, request, cancellationToken);
        return Ok(item);
    }

    [HttpDelete("items/{itemId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid itemId, CancellationToken cancellationToken)
    {
        await _itemService.DeleteAsync(itemId, cancellationToken);
        return NoContent();
    }
}
