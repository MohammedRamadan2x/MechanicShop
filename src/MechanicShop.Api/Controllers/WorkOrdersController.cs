using Asp.Versioning;

using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;
using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicShop.Application.Features.Scheduling.Queries.GetDailySchedule;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;
using MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Contracts.Requests.WorkOrders;
using MechanicShop.Application.Common.Models;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Domain.Identity;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/workorders")]
[ApiVersion("1.0")]
[Authorize]
public sealed class WorkOrdersController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<WorkOrderListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a paginated list of work orders.")]
    [EndpointDescription("Supports filtering by date range, status, vehicle, labor, spot, and searching by term. Pagination and sorting are supported.")]
    [EndpointName("GetWorkOrders")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Get(
        [FromQuery] WorkOrderFilterRequest filters,
        [FromQuery] PageRequest pageRequest,
        CancellationToken ct)
    {
        if (pageRequest.Page <= 0)
        {
            return BadRequest("Page must be greater than 0");
        }

        if (pageRequest.PageSize <= 0 || pageRequest.PageSize > 100)
        {
            return BadRequest("PageSize must be between 1 and 100");
        }

        var query = new GetWorkOrdersQuery(
            pageRequest.Page,
            pageRequest.PageSize,
            filters.SearchTerm,
            filters.SortColumn,
            filters.SortDirection,
            filters.State is not null ? (WorkOrderState)filters.State : null,
            filters.VehicleId,
            filters.LaborId,
            filters.StartDateFrom,
            filters.StartDateTo,
            filters.EndDateFrom,
            filters.EndDateTo,
            filters.Spot is not null ? (Spot)filters.Spot : null);

        var result = await sender.Send(query, ct);

        return result.Match(response => Ok(response), Problem);      
    }

    [HttpGet("{workOrderId:guid}", Name = "GetWorkOrderById")]
    [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a work order by its ID.")]
    [EndpointDescription("Returns detailed information about the specified work order if it exists.")]
    [EndpointName("GetWorkOrderById")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetById(Guid workOrderId, CancellationToken ct)
    {
        var result = await sender.Send(new GetWorkOrderByIdQuery(workOrderId), ct);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpPost]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(typeof(WorkOrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Creates a new work order.")]
    [EndpointDescription("Creates a new work order for a vehicle, specifying labor, tasks, and other required information.")]
    [EndpointName("CreateWorkOrder")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create(
        [FromBody] CreateWorkOrderRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreateWorkOrderCommand(
                (Spot)request.Spot,
                request.VehicleId,
                request.StartAtUtc,
                request.RepairTaskIds,
                request.LaborId),
            ct);

        return result.Match(
            response => CreatedAtRoute(
                routeName: "GetWorkOrderById",
                routeValues: new 
                { 
                    version = "1.0", 
                    workOrderId = response.WorkOrderId 
                },
                value: response),
            Problem);
    }

    [HttpPut("{workOrderId:guid}/relocation")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Relocates a work order to a new time and spot.")]
    [EndpointDescription("Updates the scheduled time and assigned bay for a work order. Only users with the Manager role can perform this action.")]
    [EndpointName("RescheduleWorkOrder")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Relocate(
        Guid workOrderId,
        [FromBody] RelocateWorkOrderRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new RelocateWorkOrderCommand(
                workOrderId,
                request.NewStartAtUtc,
                (Spot)request.NewSpot),
            ct);

        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPut("{workOrderId:guid}/labor")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Assigns a labor to a work order.")]
    [EndpointDescription("Associates a labor definition with a specific work order. Only managers can perform this operation.")]
    [EndpointName("AssignLaborToWorkOrder")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> AssignLabor(
        Guid workOrderId, 
        [FromBody] AssignLaborRequest request, 
        CancellationToken ct)
    {
        var result = await sender.Send(
            new AssignLaborCommand(
                workOrderId, 
                request.LaborId), 
            ct);

        return result.Match(_ => NoContent(), Problem);
    }

    [HttpPut("{workOrderId:guid}/state")]
    [Authorize
        (
            Roles = $"{nameof(Role.Manager)},{nameof(Role.Labor)}", 
            Policy = "SelfScopedWorkOrderAccess"
        )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Changes the state of a work order.")]
    [EndpointDescription("Updates the current state of the specified work order. Managers and authorized labor users can perform this operation.")]
    [EndpointName("UpdateWorkOrderState")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> UpdateState(
        Guid workOrderId, 
        [FromBody] UpdateWorkOrderStateRequest request, 
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateWorkOrderStateCommand(
                workOrderId,
                (WorkOrderState)request.State), 
            ct);

        return result.Match(_ => NoContent(), Problem);
    }


    [HttpPut("{workOrderId:guid}/repair-task")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails) ,StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Updates the repair tasks of a work order.")]
    [EndpointDescription("Replaces the repair tasks associated with the specified work order with the provided repair task IDs.")]
    [EndpointName("UpdateWorkOrderRepairTasks")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> UpdateRepairTasks(
        Guid workOrderId, 
        [FromBody] ModifyRepairTaskRequest request, 
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateWorkOrderRepairTasksCommand(
                workOrderId, 
                request.RepairTaskIds), 
            ct);

        return result.Match(_ => NoContent(), Problem);
    }

    [HttpDelete("{workOrderId:guid}")]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Deletes a work order.")]
    [EndpointDescription("Deletes the specified work order permanently. Only users with the Manager role are authorized.")]
    [EndpointName("DeleteWorkOrder")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(Guid workOrderId, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteWorkOrderCommand(workOrderId), ct);

        return result.Match(_ => NoContent(), Problem);
    }


    [HttpGet("schedule")]
    [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves the schedule for a given day.")]
    [EndpointDescription("Returns a schedule view for the specified date. If no date is provided, today's schedule is returned. You can optionally filter by labor ID.")]
    [EndpointName("GetDailySchedule")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetSchedule(
        [FromQuery] DateOnly? date,
        [FromQuery] Guid? laborId,
        [FromHeader(Name = "X-TimeZone")] string? tz,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tz))
        {
            return Problem(
                detail: "Missing time zone in 'X-TimeZone' header.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Time Zone Required");
        }

        TimeZoneInfo timeZone;

        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(tz);
        }
        catch (TimeZoneNotFoundException)
        {
            return Problem(
                detail: $"Invalid or unknown time zone: '{tz}'.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Time Zone");
        }

        var scheduleDate = date ?? DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));

        var result = await sender.Send(
            new GetDailyScheduleQuery(
                timeZone, 
                scheduleDate, 
                laborId), 
            ct);

        return result.Match(response => Ok(response), Problem);
    }
}