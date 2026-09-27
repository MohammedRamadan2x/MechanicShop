using MechanicShop.Domain.RepairTasks.Enums;

namespace MechanicShop.Application.Features.RepairTasks.Dtos;

public sealed class RepairTaskDto
{
    public Guid RepairTaskId { get; init; }
    public string Name { get; init; } = string.Empty;
    public RepairDurationInMinutes EstimatedDurationInMins { get; init; }
    public decimal LaborCost { get; init; }
    public decimal TotalCost { get; init; }
    public List<PartDto> Parts { get; init; } = [];
}