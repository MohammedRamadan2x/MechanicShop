using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;

public class AssignLaborCommandHandler(
    ILogger<AssignLaborCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    IWorkOrderPolicy workOrderPolicy)
    : IRequestHandler<AssignLaborCommand, Result<Updated>>
{
    private readonly ILogger<AssignLaborCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly IWorkOrderPolicy _workOrderPolicy = workOrderPolicy;

    public async Task<Result<Updated>> Handle(
        AssignLaborCommand command, 
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

        var labor = await _context.Employees
            .FindAsync(command.LaborId, ct);

        if (labor is null)
        {
            _logger.LogWarning("Invalid LaborId: {LaborId}", command.LaborId);

            return ApplicationErrors.LaborNotFound;
        }

        if (await _workOrderPolicy.IsLaborOccupied(
            labor.Id, 
            workOrder.Id, 
            workOrder.StartAtUtc, 
            workOrder.EndAtUtc))
        {
            _logger.LogWarning(
                "Labor with Id '{LaborId}' is already occupied during the " +
                "requested time.", 
                labor.Id);

            return ApplicationErrors.LaborOccupied;
        }

        var assignLaborResult = workOrder.UpdateLabor(labor.Id);

        if (assignLaborResult.IsFailure)
        {
            foreach (var error in assignLaborResult.Errors)
            {
                _logger.LogWarning(
                    "[AssignLabor] {ErrorCode}: {ErrorDescription}", 
                    error.Code, 
                    error.Description);
            }

            return assignLaborResult.Errors;
        }

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("work-order", ct);

        _logger.LogInformation(
            "Labor '{LaborId}' assigned successfully to WorkOrder '{WorkOrderId}'.",
            labor.Id,
            workOrder.Id);

        return Result.Updated;
    }
}
