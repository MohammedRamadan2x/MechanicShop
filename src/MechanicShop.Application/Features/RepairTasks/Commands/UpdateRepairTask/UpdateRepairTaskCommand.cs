using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;

public sealed record UpdateRepairTaskCommand(
    Guid RepairTaskId,
    string Name,
    decimal LaborCost,
    RepairDurationInMinutes EstimatedDurationInMins,
    List<UpdateRepairTaskPartCommand> Parts) 
    : IRequest<Result<Updated>>;