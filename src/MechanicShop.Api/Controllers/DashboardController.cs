using Asp.Versioning;

using MechanicShop.Application.Features.Dashboard.Queries;
using MechanicShop.Application.Features.Dashboard.Dtos;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/dashboard")]
[ApiVersion("1.0")]
[Authorize]
public sealed class DashboardController(ISender sender) : ApiController
{
    [HttpGet("stats")]
    [ProducesResponseType(typeof(TodayWorkOrderStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTodayStats(
        [FromQuery] DateOnly? date, 
        CancellationToken ct)
    {
        var statsDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await sender.Send(new GetWorkOrderStatsQuery(statsDate), ct);

        return result.Match(response => Ok(response), Problem);
    }
}