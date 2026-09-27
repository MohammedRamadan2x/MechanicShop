using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Domain.WorkOrders.Events;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public class CreateWorkOrderCommandHandler(
    ILogger<CreateWorkOrderCommandHandler> logger,
    IAppDbContext context,
    IWorkOrderPolicy workOrderPolicy,
    HybridCache cache)
    : IRequestHandler<CreateWorkOrderCommand, Result<WorkOrderDto>>
{
    private readonly ILogger<CreateWorkOrderCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly IWorkOrderPolicy _workOrderPolicy = workOrderPolicy;
    private readonly HybridCache _cache = cache;

    public async Task<Result<WorkOrderDto>> Handle(
        CreateWorkOrderCommand command, 
        CancellationToken ct)
    {
        var repairTasks = await _context.RepairTasks
            .Where(rt => command.RepairTaskIds.Contains(rt.Id))
            .ToListAsync(ct);

        if (repairTasks.Count != command.RepairTaskIds.Count)
        {
            var missingIds = command.RepairTaskIds
                .Except(repairTasks.Select(rt => rt.Id))
                .ToArray();

            _logger.LogWarning(
                "Some RepairTaskIds not found: {MissingIds}", 
                string.Join(", ", missingIds));

            return ApplicationErrors.RepairTaskNotFound;
        }

        var totalEstimatedDuration = TimeSpan.FromMinutes(repairTasks
            .Sum(rt => (int)rt.EstimatedDurationInMins));

        var endAt = command.StartAt.Add(totalEstimatedDuration);

        if (_workOrderPolicy.IsOutsideOperatingHours(
            command.StartAt, 
            totalEstimatedDuration))
        {
            _logger.LogWarning(
                "The WorkOrder time ({StartAt} ? {EndAt}) is outside of store " +
                "operating hours.", 
                command.StartAt, 
                endAt);

            return ApplicationErrors.WorkOrderOutsideOperatingHour(
                command.StartAt, 
                endAt);
        }

        var checkMinimumRequirementResult = _workOrderPolicy
            .ValidateMinimumRequirement(command.StartAt, endAt);

        if (checkMinimumRequirementResult.IsFailure)
        {
            _logger.LogWarning(
                "WorkOrder duration is shorter than the configured minimum.");

            return checkMinimumRequirementResult.Errors;
        }

        var checkSpotAvailabilityResult = await _workOrderPolicy
            .CheckSpotAvailabilityAsync(
            command.Spot, 
            command.StartAt, 
            endAt,
            excludeWorkOrderId: null, 
            ct);

        if (checkSpotAvailabilityResult.IsFailure)
        {
            _logger.LogWarning(
                "Spot: {Spot} is not available.", 
                command.Spot.ToString());

            return checkSpotAvailabilityResult.Errors;
        }

        var vehicle = await _context.Vehicles
            .Include(v => v.Customer)
            .FirstOrDefaultAsync(v => v.Id == command.VehicleId, ct);

        if (vehicle is null)
        {
            _logger.LogWarning(
                "Vehicle with Id '{VehicleId}' does not exist.", 
                command.VehicleId);

            return ApplicationErrors.VehicleNotFound;
        }

        var labor = await _context.Employees
            .FindAsync([command.LaborId], ct);

        if (labor is null)
        {
            _logger.LogWarning(
                "Invalid LaborId: {LaborId}", 
                command.LaborId);

            return ApplicationErrors.LaborNotFound;
        }

        var hasVehicleConflict = await _context.WorkOrders
            .AnyAsync(wo => 
                wo.VehicleId == command.VehicleId &&
                wo.StartAtUtc.Date == command.StartAt.Date &&
                wo.StartAtUtc < endAt &&
                wo.EndAtUtc > command.StartAt,
                ct);

        if (hasVehicleConflict)
        {
            _logger.LogWarning(
                "Vehicle with Id '{VehicleId}' already has an overlapping WorkOrder.", 
                command.VehicleId);
            
            return Error.Conflict(
                code: "Vehicle.Overlapping.WorkOrders",
                description: "The vehicle already has an overlapping WorkOrder.");
        }

        var isLaborOccupied = await _context.WorkOrders
            .AnyAsync(wo =>
                wo.LaborId == command.LaborId &&
                wo.StartAtUtc < endAt &&
                wo.EndAtUtc > command.StartAt,
                ct);

        if (isLaborOccupied)
        {
            _logger.LogWarning(
                "Labor with Id '{LaborId}' is already occupied during the " +
                "requested time.", 
                command.LaborId);
            
            return Error.Conflict(
                code: "Labor.Occupied",
                description: "Labor is already occupied during the requested time.");
        }

        var createWorkOrderResult = WorkOrder.Create(
            Guid.NewGuid(),
            vehicle.Id,
            command.StartAt,
            endAt,
            labor.Id,
            command.Spot,
            repairTasks);

        if (createWorkOrderResult.IsFailure)
        {
            _logger.LogError(
                "Failed to create WorkOrder: {Error}", 
                createWorkOrderResult.TopError.Description);

            return createWorkOrderResult.Errors;
        }

        var workOrder = createWorkOrderResult.Value;

        _context.WorkOrders.Add(workOrder);

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("work-order", ct);
        
        workOrder.Vehicle = vehicle;
        workOrder.Labor = labor;
        
        _logger.LogInformation(
            "WorkOrder with Id '{WorkOrderId}' created successfully.", 
            workOrder.Id);

        return workOrder.ToDto();
    }
}