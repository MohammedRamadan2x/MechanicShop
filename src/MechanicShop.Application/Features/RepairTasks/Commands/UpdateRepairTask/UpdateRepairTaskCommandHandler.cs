using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;

public class UpdateRepairTaskCommandHandler(
    ILogger<UpdateRepairTaskCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache)
    : IRequestHandler<UpdateRepairTaskCommand, Result<Updated>>
{
    private readonly ILogger<UpdateRepairTaskCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Updated>> Handle(
        UpdateRepairTaskCommand command, 
        CancellationToken ct)
    {
        var repairTask = await _context.RepairTasks
            .Include(rt => rt.Parts)
            .FirstOrDefaultAsync(rt => rt.Id == command.RepairTaskId, ct);

        if (repairTask is null)
        {
            _logger.LogWarning(
                "RepairTask {RepairTaskId} not found for update.", 
                command.RepairTaskId);

            return ApplicationErrors.RepairTaskNotFound;
        }

        var isDuplicateName = await _context.RepairTasks
            .AnyAsync(rt =>
                rt.Id != command.RepairTaskId &&
                EF.Functions.Like(rt.Name, command.Name),
                ct);

        if (isDuplicateName)
        {
            _logger.LogWarning(
                "Duplicate repair task name '{RepairTaskName}'.",
                command.Name);

            return RepairTaskErrors.DuplicateName;
        }

        var validatedParts = new List<Part>();

        foreach (var p in command.Parts)
        {
            if (p.PartId.HasValue)
            {
                var existingPart = repairTask.Parts
                    .FirstOrDefault(part => part.Id == p.PartId.Value);

                if (existingPart is null)
                {
                    return ApplicationErrors.PartNotFound;
                }

                var updatePartResult = existingPart.Update(
                    p.Name,
                    p.Cost,
                    p.Quantity);

                if (updatePartResult.IsFailure)
                {
                    return updatePartResult.Errors;
                }

                validatedParts.Add(existingPart);
            }
            else
            {
                var createPartResult = Part.Create(
                    Guid.NewGuid(),
                    p.Name,
                    p.Cost,
                    p.Quantity);

                if (createPartResult.IsFailure)
                {
                    return createPartResult.Errors;
                }

                validatedParts.Add(createPartResult.Value);
            }
        }

        var updateRepairTaskResult = repairTask.Update(
            command.Name,
            command.LaborCost,
            command.EstimatedDurationInMins);

        if (updateRepairTaskResult.IsFailure)
        {
            return updateRepairTaskResult.Errors;
        }

        var upsertPartsResult = repairTask.UpsertParts(validatedParts);

        if (upsertPartsResult.IsFailure)
        {
            _logger.LogWarning(
                "Failed to update parts for repair task '{RepairTaskName}'. " +
                "Error: {ErrorDescription}",
                repairTask.Name,
                upsertPartsResult.TopError.Description);

            return upsertPartsResult.Errors;
        }

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("repair-tasks", ct);

        _logger.LogInformation(
            "Repair task '{RepairTaskName}' updated successfully. " +
            "Id: {RepairTaskId}.",
            repairTask.Name,
            repairTask.Id);

        return Result.Updated;
    }
}