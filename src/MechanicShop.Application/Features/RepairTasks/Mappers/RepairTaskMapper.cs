using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Domain.RepairTasks;

namespace MechanicShop.Application.Features.RepairTasks.Mappers;

public static class RepairTaskMapper
{
    public static RepairTaskDto ToDto(this RepairTask repairTask)
    {
        ArgumentNullException.ThrowIfNull(repairTask);

        return new RepairTaskDto
        {
            RepairTaskId = repairTask.Id,
            Name = repairTask.Name,
            LaborCost = repairTask.LaborCost,
            TotalCost = repairTask.TotalCost,
            EstimatedDurationInMins = repairTask.EstimatedDurationInMins,
            Parts = repairTask.Parts.Select(p => p.ToDto()).ToList()
        };
    }

    public static List<RepairTaskDto> ToDtos(this IEnumerable<RepairTask> repairTasks)
    {
        return [.. repairTasks.Select(e => e.ToDto())];
    }

    public static PartDto ToDto(this Part part)
    {
        ArgumentNullException.ThrowIfNull(part);

        return new PartDto
        {
            PartId = part.Id,
            Name = part.Name!,
            Cost = part.Cost,
            Quantity = part.Quantity
        };
    }

    public static List<PartDto> ToDtos(this IEnumerable<Part> parts)
    {
        return [.. parts.Select(p => p.ToDto())];
    }
}