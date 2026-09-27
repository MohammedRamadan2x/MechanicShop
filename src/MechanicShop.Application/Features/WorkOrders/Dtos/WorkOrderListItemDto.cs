using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.WorkOrders.Dtos;

public sealed class WorkOrderListItemDto
{
    public Guid WorkOrderId { get; init; }
    public Guid? InvoiceId { get; init; }
    public VehicleDto Vehicle { get; init; } = default!;
    public string? Customer { get; init; }
    public string? Labor { get; init; }
    public WorkOrderState State { get; init; }
    public Spot Spot { get; init; }
    public DateTimeOffset StartAtUtc { get; init; }
    public DateTimeOffset EndAtUtc { get; init; }
    public List<string> RepairTasks { get; init; } = [];
}