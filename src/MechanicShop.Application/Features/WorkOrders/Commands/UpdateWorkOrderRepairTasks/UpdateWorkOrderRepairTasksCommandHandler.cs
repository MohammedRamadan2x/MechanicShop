using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Domain.WorkOrders.Events;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public class UpdateWorkOrderRepairTasksCommandHandler(
    ILogger<UpdateWorkOrderRepairTasksCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    IWorkOrderPolicy workOrderPolicy)
    : IRequestHandler<UpdateWorkOrderRepairTasksCommand, Result<Updated>>
{
    private readonly ILogger<UpdateWorkOrderRepairTasksCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly IWorkOrderPolicy _workOrderPolicy = workOrderPolicy;
    
    public async Task<Result<Updated>> Handle(
        UpdateWorkOrderRepairTasksCommand command, 
        CancellationToken ct)
    {
        var workOrder = await _context.WorkOrders
            .Include(wo => wo.RepairTasks)
            .FirstOrDefaultAsync(wo => wo.Id == command.WorkOrderId, ct);

        if (workOrder is null)
        {
            _logger.LogWarning(
                "WorkOrder with Id '{WorkOrderId}' does not exist.", 
                command.WorkOrderId);

            return ApplicationErrors.WorkOrderNotFound;
        }

        if (command.RepairTaskIds.Length == 0)
        {
            _logger.LogWarning("Empty RepairTaskIds list submitted.");

            return RepairTaskErrors.AtLeastOneRepairTaskIsRequired;
        }

        var requestedTasks = await _context.RepairTasks
            .Where(rt => command.RepairTaskIds.Contains(rt.Id))
            .ToListAsync(ct);

        if (requestedTasks.Count != command.RepairTaskIds.Length)
        {
            var missingIds = command.RepairTaskIds
                .Except(requestedTasks.Select(rt => rt.Id))
                .ToArray();

            _logger.LogWarning(
                "One or more RepairTasks not found. {ids}", 
                string.Join(", ", missingIds));

            return ApplicationErrors.RepairTaskNotFound;
        }

        var totalDuration = TimeSpan.FromMinutes(
            requestedTasks.Sum(rt => (int)rt.EstimatedDurationInMins));

        var newEndAt = workOrder.StartAtUtc.Add(totalDuration);

        // Business validations
        if (_workOrderPolicy.IsOutsideOperatingHours(
            workOrder.StartAtUtc, 
            totalDuration))
        {
            _logger.LogWarning(
                "Updated RepairTasks would move WorkOrder '{WorkOrderId}' " +
                "outside operating hours.",
                workOrder.Id);

            return ApplicationErrors.WorkOrderOutsideOperatingHour(
                workOrder.StartAtUtc, 
                newEndAt);
        }

        var checkMinimumRequirementResult = _workOrderPolicy
            .ValidateMinimumRequirement(
            workOrder.StartAtUtc,
            newEndAt);

        if (checkMinimumRequirementResult.IsFailure)
        {
            _logger.LogWarning(
                "Updated RepairTasks violate the minimum duration requirement " +
                "for WorkOrder '{WorkOrderId}'.",
                workOrder.Id);

            return checkMinimumRequirementResult.Errors;
        }

        if (await _workOrderPolicy.IsVehicleAlreadyScheduled(
            workOrder.VehicleId,
            workOrder.StartAtUtc,
            newEndAt,
            workOrder.Id))
        {
            _logger.LogWarning(
                "Vehicle '{VehicleId}' already has an overlapping WorkOrder.",
                workOrder.VehicleId);

            return ApplicationErrors.VehicleSchedulingConflict;
        }

        if (await _workOrderPolicy.IsLaborOccupied(
            workOrder.LaborId, 
            workOrder.Id, 
            workOrder.StartAtUtc, 
            newEndAt))
        {
            _logger.LogWarning(
                "Labor '{LaborId}' is occupied during the updated schedule for " +
                "WorkOrder '{WorkOrderId}'.",
                workOrder.LaborId,
                workOrder.Id);

            return ApplicationErrors.LaborOccupied;
        }

        var checkSpotAvailabilityResult = await _workOrderPolicy
            .CheckSpotAvailabilityAsync(
            workOrder.Spot,
            workOrder.StartAtUtc,
            newEndAt,
            excludeWorkOrderId: workOrder.Id,
            ct);

        if (checkSpotAvailabilityResult.IsFailure)
        {
            _logger.LogWarning(
                "Spot '{Spot}' is unavailable for WorkOrder '{WorkOrderId}'.",
                workOrder.Spot,
                workOrder.Id);

            return checkSpotAvailabilityResult.Errors;
        }

        var clearExistingResult = workOrder.ClearRepairTasks();

        if (clearExistingResult.IsFailure)
        {
            _logger.LogWarning(
                "Failed to clear RepairTasks for WorkOrder '{WorkOrderId}': {Error}",
                workOrder.Id,
                clearExistingResult.TopError.Description);

            return clearExistingResult;
        }

        foreach (var task in requestedTasks)
        {
            var addRepairTaskResult = workOrder.AddRepairTask(task);

            if (addRepairTaskResult.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to add RepairTask '{RepairTaskId}' to WorkOrder " +
                    "'{WorkOrderId}': {Error}",
                    task.Id,
                    workOrder.Id,
                    addRepairTaskResult.TopError.Description);

                return addRepairTaskResult.Errors;
            }
        }

        var updateTimingResult = workOrder.UpdateTiming(
            workOrder.StartAtUtc, 
            newEndAt);

        if (updateTimingResult.IsFailure)
        {
            _logger.LogWarning(
                "Failed to update timing for WorkOrder '{WorkOrderId}': {Error}",
                workOrder.Id,
                updateTimingResult.TopError.Description);

            return updateTimingResult.Errors;
        }

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("work-order", ct);

        _logger.LogInformation(
            "RepairTasks for WorkOrder '{WorkOrderId}' updated successfully.",
            workOrder.Id);

        return Result.Updated;
    }
}
