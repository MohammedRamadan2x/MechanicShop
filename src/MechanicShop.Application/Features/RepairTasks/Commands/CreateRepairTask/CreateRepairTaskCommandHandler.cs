using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;


public class CreateRepairTaskCommandHandler(
    ILogger<CreateRepairTaskCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache)
    : IRequestHandler<CreateRepairTaskCommand, Result<RepairTaskDto>>
{
    private readonly ILogger<CreateRepairTaskCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<RepairTaskDto>> Handle(
        CreateRepairTaskCommand command, 
        CancellationToken ct)
    {
        var nameExists = await _context.RepairTasks
           .AnyAsync(rt => EF.Functions.Like(rt.Name, command.Name), ct);

        if (nameExists)
        {
            _logger.LogWarning(
                "Duplicate repair task name '{RepairTaskName}'.",
                command.Name);

            return RepairTaskErrors.DuplicateName;
        }

        List<Part> parts = [];

        foreach (var p in command.Parts)
        {
            var createPartResult = Part.Create(
                Guid.NewGuid(), 
                p.Name, 
                p.Cost, 
                p.Quantity);

            if (createPartResult.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to create part '{PartName}' for " +
                    "repair task '{RepairTaskName}'. " +
                    "Error: {ErrorDescription}",
                    p.Name,
                    command.Name,
                    createPartResult.TopError.Description);

                return createPartResult.Errors;
            }

            parts.Add(createPartResult.Value);
        }

        var createRepairTaskResult = RepairTask.Create(
                    id: Guid.NewGuid(),
                    name: command.Name,
                    laborCost: command.LaborCost,
                    estimatedDurationInMins: command.EstimatedDurationInMins,
                    parts: parts);

        if (createRepairTaskResult.IsFailure)
        {
            _logger.LogWarning(
                "Failed to create repair task '{RepairTaskName}'. " +
                "Error: {ErrorDescription}",
                command.Name,
                createRepairTaskResult.TopError.Description);

            return createRepairTaskResult.Errors;
        }

        var repairTask = createRepairTaskResult.Value;

        _context.RepairTasks.Add(repairTask);

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("repair-tasks", ct);

        _logger.LogInformation(
            "Repair task '{RepairTaskName}' created successfully with " +
            "Id {RepairTaskId}.",
            repairTask.Name,
            repairTask.Id);

        return repairTask.ToDto();
    }
}