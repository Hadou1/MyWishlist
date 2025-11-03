using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wishlist.Application.Interfaces;
using Wishlist.Contracts.Gdpr;
using Wishlist.Contracts.Users;

namespace Wishlist.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/me")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var user = await _userService.GetCurrentUserAsync(cancellationToken);
        return Ok(user);
    }

    [HttpGet("export")]
    [ProducesResponseType(typeof(GdprExportResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var export = await _userService.ExportAsync(cancellationToken);
        return Ok(export);
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken)
    {
        await _userService.DeleteCurrentUserAsync(cancellationToken);
        return NoContent();
    }
}
