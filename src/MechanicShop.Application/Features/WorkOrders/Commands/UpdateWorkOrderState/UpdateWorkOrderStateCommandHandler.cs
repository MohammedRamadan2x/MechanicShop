using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Domain.WorkOrders.Events;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;

public class UpdateWorkOrderStateCommandHandler(
    ILogger<UpdateWorkOrderStateCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    TimeProvider timeProvider)
    : IRequestHandler<UpdateWorkOrderStateCommand, Result<Updated>>
{
    private readonly ILogger<UpdateWorkOrderStateCommandHandler> _logger= logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result<Updated>> Handle(
        UpdateWorkOrderStateCommand command, 
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

        if (workOrder.StartAtUtc > _timeProvider.GetUtcNow())
        {
            _logger.LogWarning(
                "State transition for WorkOrder Id '{WorkOrderId}' is not allowed " +
                "before the work order's scheduled start time.", 
                command.WorkOrderId);

            return WorkOrderErrors.StateTransitionNotAllowed(workOrder.StartAtUtc);
        }

        var updateStateResult = workOrder.UpdateState(command.State);

        if (updateStateResult.IsFailure)
        {
            _logger.LogWarning(
                "Failed to update work order state: {Error}",
                updateStateResult.TopError.Description);

            return updateStateResult.Errors;
        }

        if (command.State == WorkOrderState.Completed)
        {
            workOrder.AddDomainEvent(new WorkOrderCompleted(workOrder.Id));
        }

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("work-order", ct);

        _logger.LogInformation(
            "WorkOrder '{WorkOrderId}' state updated successfully to '{State}'.",
            workOrder.Id,
            command.State);

        return Result.Updated;
    }
}
