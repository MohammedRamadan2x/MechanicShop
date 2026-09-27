using Asp.Versioning;

using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;
using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Contracts.Requests.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/repair-tasks")]
[ApiVersion("1.0")]
[Authorize]
public sealed class RepairTasksController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<RepairTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves all repair tasks.")]
    [EndpointDescription("Returns a list of all repair tasks available in the system.")]
    [EndpointName("GetRepairTasks")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await sender.Send(new GetRepairTasksQuery(), ct);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpGet("{repairTaskId:guid}", Name = nameof(GetById))]
    [ProducesResponseType(typeof(RepairTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a repair task by ID.")]
    [EndpointDescription("Returns detailed information for the specified repair task if it exists.")]
    [EndpointName("GetRepairTaskById")]
    [MapToApiVersion("1.0")]
    [OutputCache(Duration = 60)]
    public async Task<IActionResult> GetById(Guid repairTaskId, CancellationToken ct)
    {
        var result = await sender.Send(new GetRepairTaskByIdQuery(repairTaskId), ct);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpPost]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(RepairTaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Creates a new repair task.")]
    [EndpointDescription("Creates a repair task with its associated parts.")]
    [EndpointName("CreateRepairTask")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create(
        [FromBody] CreateRepairTaskRequest request, 
        CancellationToken ct)
    {
        var parts = request.Parts
        .ConvertAll(p => new CreateRepairTaskPartCommand(p.Name, p.Cost, p.Quantity));

        var result = await sender.Send(
            new CreateRepairTaskCommand(
                request.Name,
                request.LaborCost,
                (RepairDurationInMinutes)request.EstimatedDurationInMins,
                parts), 
            ct);

        return result.Match(
            response => 
            CreatedAtAction(
                nameof(GetById), 
                new 
                {
                    version = "1.0",
                    repairTaskId = response.RepairTaskId 
                }, 
                response),
            Problem);
    }

    [HttpPut("{repairTaskId:guid}")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Updates an existing repair task.")]
    [EndpointDescription("Updates a repair task and its associated parts.")]
    [EndpointName("UpdateRepairTask")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Update(
        Guid repairTaskId, 
        [FromBody] UpdateRepairTaskRequest request, 
        CancellationToken ct)
    {
        var parts = request.Parts
            .ConvertAll(p => new UpdateRepairTaskPartCommand(
                p.PartId, 
                p.Name, 
                p.Cost, 
                p.Quantity));

        var result = await sender.Send(
            new UpdateRepairTaskCommand(
                repairTaskId,
                request.Name,
                request.LaborCost,
                (RepairDurationInMinutes)request.EstimatedDurationInMins,
                parts), 
            ct);

        return result.Match(_ => NoContent(), Problem);
    }

    [HttpDelete("{repairTaskId:guid}")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Removes a repair task.")]
    [EndpointDescription("Deletes the specified repair task from the system.")]
    [EndpointName("RemoveRepairTask")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(Guid repairTaskId, CancellationToken ct)
    {
        var result = await sender.Send(new RemoveRepairTaskCommand(repairTaskId), ct);

        return result.Match(_ => NoContent(), Problem);
    }
}