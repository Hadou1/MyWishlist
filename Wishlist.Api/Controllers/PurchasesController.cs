using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wishlist.Application.Interfaces;
using Wishlist.Contracts.Purchases;

namespace Wishlist.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;

    public PurchasesController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    [HttpPost("items/{itemId:guid}/purchases")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PurchaseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(Guid itemId, [FromBody] PurchaseCreateRequest request, CancellationToken cancellationToken)
    {
        var purchase = await _purchaseService.CreateAsync(itemId, request, cancellationToken);
        return CreatedAtAction(nameof(GetForItem), new { itemId }, purchase);
    }

    [HttpGet("items/{itemId:guid}/purchases")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<PurchaseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForItem(Guid itemId, CancellationToken cancellationToken)
    {
        var purchases = await _purchaseService.GetForItemAsync(itemId, cancellationToken);
        return Ok(purchases);
    }
}
