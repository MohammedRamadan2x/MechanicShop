using Asp.Versioning;

using MechanicShop.Application.Features.Labors.Queries.GetLabors;
using MechanicShop.Application.Features.Labors.Dtos;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/labors")]
[ApiVersion("1.0")]
[Authorize]
public sealed class LaborsController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<LaborDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves the list of available labor definitions.")]
    [EndpointDescription("Returns all labor records associated with the system, accessible only to users with the Manager role.")]
    [EndpointName("GetLabors")]
    [MapToApiVersion("1.0")]
    [OutputCache(Duration = 60)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await sender.Send(new GetLaborsQuery(), ct);

        return result.Match(response => Ok(response), Problem);
    }
}