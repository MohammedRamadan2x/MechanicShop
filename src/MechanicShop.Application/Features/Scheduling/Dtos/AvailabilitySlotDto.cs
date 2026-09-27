using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.Labors.Dtos;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.Scheduling.Dtos;

public sealed class AvailabilitySlotDto
{
    public Guid? WorkOrderId { get; init; }
    public Spot Spot { get; init; }
    public DateTimeOffset StartAt { get; init; }
    public DateTimeOffset EndAt { get; init; }
    public string? Vehicle { get; init; }
    public LaborDto? Labor { get; init; }
    public bool IsOccupied { get; init; }
    public bool? IsAvailable { get; init; }
    public bool WorkOrderLocked { get; init; }
    public WorkOrderState? State { get; init; }
    public RepairTaskDto[]? RepairTasks { get; init; }
}