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

namespace MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;


public class DeleteWorkOrderCommandHandler(
    ILogger<DeleteWorkOrderCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache)
    : IRequestHandler<DeleteWorkOrderCommand, Result<Deleted>>
{
    private readonly ILogger<DeleteWorkOrderCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Deleted>> Handle(
        DeleteWorkOrderCommand command, 
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

        if (workOrder.State is not WorkOrderState.Scheduled)
        {
            _logger.LogWarning(
                "Deletion failed: Only Scheduled WorkOrders can be deleted. " +
                "Current status: {Status}",
                workOrder.State);

            return WorkOrderErrors.ReadOnly;
        }

        _context.WorkOrders.Remove(workOrder);

        workOrder.AddDomainEvent(new WorkOrderCollectionModified());
        
        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("work-order", ct);

        _logger.LogInformation(
            "WorkOrder '{WorkOrderId}' deleted successfully.",
            workOrder.Id);

        return Result.Deleted;
    }
}