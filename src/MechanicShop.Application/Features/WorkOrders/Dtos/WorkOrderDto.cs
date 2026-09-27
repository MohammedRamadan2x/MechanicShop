using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Labors.Dtos;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.WorkOrders.Dtos;

public sealed class WorkOrderDto
{
    public Guid WorkOrderId { get; init; }
    public Guid? InvoiceId { get; init; }
    public Spot Spot { get; init; }
    public VehicleDto? Vehicle { get; init; }
    public DateTimeOffset StartAtUtc { get; init; }
    public DateTimeOffset EndAtUtc { get; init; }
    public List<RepairTaskDto> RepairTasks { get; init; } = [];
    public LaborDto? Labor { get; init; }
    public WorkOrderState State { get; init; }
    public decimal TotalPartCost { get; init; }
    public decimal TotalLaborCost { get; init; }
    public decimal TotalCost { get; init; }
    public int TotalDurationInMins { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}