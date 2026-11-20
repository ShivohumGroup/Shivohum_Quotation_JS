using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShivohumExpenseTracker.Api.Models.Dtos;
using ShivohumExpenseTracker.Api.Services;

namespace ShivohumExpenseTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DashboardController(DashboardService dashboard) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(
        [FromQuery] string? preset,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        return Ok(await dashboard.GetAsync(preset, from, to));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> Summary(
        [FromQuery] string? preset,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var data = await dashboard.GetAsync(preset, from, to);
        return Ok(data.Summary);
    }
}
