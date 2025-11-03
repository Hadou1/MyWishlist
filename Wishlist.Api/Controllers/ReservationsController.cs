using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wishlist.Application.Interfaces;
using Wishlist.Contracts.Reservations;

namespace Wishlist.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost("items/{itemId:guid}/reservations")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(Guid itemId, [FromBody] ReservationCreateRequest request, CancellationToken cancellationToken)
    {
        var reservation = await _reservationService.CreateAsync(itemId, request, cancellationToken);
        return CreatedAtAction(nameof(GetForItem), new { itemId }, reservation);
    }

    [HttpGet("items/{itemId:guid}/reservations")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForItem(Guid itemId, CancellationToken cancellationToken)
    {
        var reservations = await _reservationService.GetForItemAsync(itemId, cancellationToken);
        return Ok(reservations);
    }

    [HttpDelete("reservations/{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _reservationService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
