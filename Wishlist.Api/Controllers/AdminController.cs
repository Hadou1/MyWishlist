using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wishlist.Application.Common;
using Wishlist.Application.Interfaces;
using Wishlist.Contracts.Admin;
using Wishlist.Contracts.Common;

namespace Wishlist.Api.Controllers;

[ApiController]
[Authorize(Policy = "Admin")]
[Route("api/v1/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResult<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] string? sort, [FromQuery(Name = "q")] string? search, CancellationToken cancellationToken)
    {
        var query = new QueryParameters(page, pageSize, sort, search);
        var result = await _adminService.GetUsersAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("lists")]
    [ProducesResponseType(typeof(PagedResult<AdminListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLists([FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] string? sort, [FromQuery(Name = "q")] string? search, CancellationToken cancellationToken)
    {
        var query = new QueryParameters(page, pageSize, sort, search);
        var result = await _adminService.GetListsAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("reports")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Report([FromBody] AbuseReportRequest request, CancellationToken cancellationToken)
    {
        await _adminService.ReportAsync(request, cancellationToken);
        return Accepted();
    }

    [HttpPost("moderate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Moderate([FromBody] ModerationRequest request, CancellationToken cancellationToken)
    {
        await _adminService.ModerateAsync(request, cancellationToken);
        return Accepted();
    }
}
