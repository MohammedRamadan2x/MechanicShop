using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Domain.WorkOrders.Events;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;


public class RelocateWorkOrderCommandHandler(
    ILogger<RelocateWorkOrderCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    IWorkOrderPolicy workOrderPolicy)
    : IRequestHandler<RelocateWorkOrderCommand, Result<Updated>>
{
    private readonly ILogger<RelocateWorkOrderCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly IWorkOrderPolicy _workOrderPolicy = workOrderPolicy;

    public async Task<Result<Updated>> Handle(
        RelocateWorkOrderCommand command, 
        CancellationToken ct)
    {
        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(wo => wo.Id == command.WorkOrderId, ct);

        if (workOrder is null)
        {
            _logger.LogWarning(
                "WorkOrder with Id '{WorkOrderId}' does not exist.", 
                command.WorkOrderId);

            return ApplicationErrors.WorkOrderNotFound;
        }

        var duration = workOrder.EndAtUtc.Subtract(workOrder.StartAtUtc).Duration();

        var endAt = command.NewStartAt.Add(duration);

        var checkSpotAvailabilityResult = await _workOrderPolicy
            .CheckSpotAvailabilityAsync(
            command.NewSpot,
            command.NewStartAt,
            endAt,
            excludeWorkOrderId: workOrder.Id,
            ct);

        if (checkSpotAvailabilityResult.IsFailure)
        {
            _logger.LogWarning(
                "Spot: {Spot} is not available.", 
                command.NewSpot.ToString());

            return checkSpotAvailabilityResult.Errors;
        }

        if (await _workOrderPolicy.IsLaborOccupied(
            workOrder.LaborId, 
            workOrder.Id, 
            command.NewStartAt, 
            endAt))
        {
            _logger.LogWarning(
                "Labor with Id '{LaborId}' is already occupied during the " +
                "requested time.", 
                workOrder.LaborId);

            return ApplicationErrors.LaborOccupied;
        }

        if (await _workOrderPolicy.IsVehicleAlreadyScheduled(
            workOrder.VehicleId, 
            command.NewStartAt, 
            endAt, 
            workOrder.Id))
        {
            _logger.LogWarning(
                "Vehicle with Id '{VehicleId}' already has an overlapping WorkOrder.", 
                workOrder.VehicleId);

            return ApplicationErrors.VehicleSchedulingConflict;
        }

        var updateTimingResult = workOrder.UpdateTiming(command.NewStartAt, endAt);

        if (updateTimingResult.IsFailure)
        {
            _logger.LogWarning(
                "Failed to update timing: {Error}", 
                updateTimingResult.TopError.Description);

            return updateTimingResult.Errors;
        }

        var updateSpotResult = workOrder.UpdateSpot(command.NewSpot);

        if (updateSpotResult.IsFailure)
        {
            _logger.LogWarning(
                "Failed to update Spot: {Error}", 
                updateSpotResult.TopError.Description);

            return updateSpotResult.Errors;
        }

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("work-order", ct);

        _logger.LogInformation(
            "WorkOrder '{WorkOrderId}' relocated successfully. " +
            "New start: {NewStartAt}, New end: {EndAt}, New spot: {NewSpot}.",
            workOrder.Id,
            command.NewStartAt,
            endAt,
            command.NewSpot);

        return Result.Updated;
    }
}