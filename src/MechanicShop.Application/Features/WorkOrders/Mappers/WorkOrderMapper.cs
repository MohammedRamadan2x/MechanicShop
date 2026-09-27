using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.Labors.Mappers;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.Features.WorkOrders.Mappers;

public static class WorkOrderMapper
{
    public static WorkOrderDto ToDto(this WorkOrder workOrder)
    {
        ArgumentNullException.ThrowIfNull(workOrder);

        return new WorkOrderDto
        {
            WorkOrderId = workOrder.Id,
            Vehicle = workOrder.Vehicle is null ? null : workOrder.Vehicle.ToDto(),
            TotalPartCost = workOrder.RepairTasks.SelectMany(re => re.Parts).Sum(p => p.Quantity * p.Cost),
            TotalLaborCost = workOrder.RepairTasks.Sum(rt => rt.LaborCost),
            TotalDurationInMins = workOrder.RepairTasks.Sum(rt => (int)rt.EstimatedDurationInMins),
            TotalCost = workOrder.RepairTasks.Sum(rt => rt.TotalCost),
            State = workOrder.State,
            Spot = workOrder.Spot,
            RepairTasks = workOrder.RepairTasks.ToDtos(),
            Labor = workOrder.Labor is null ? null : workOrder.Labor.ToDto(),
            InvoiceId = workOrder.Invoice?.Id,
            StartAtUtc = workOrder.StartAtUtc,
            EndAtUtc = workOrder.EndAtUtc,
            CreatedAt = workOrder.CreatedAtUtc
        };
    }

    public static List<WorkOrderDto> ToDtos(this IEnumerable<WorkOrder> workOrders)
    {
        return [.. workOrders.Select(wo => wo.ToDto())];
    }

    public static WorkOrderListItemDto ToListItemDto(this WorkOrder workOrder)
    {
        ArgumentNullException.ThrowIfNull(workOrder);

        return new WorkOrderListItemDto
        {
            WorkOrderId = workOrder.Id,
            Vehicle = workOrder.Vehicle!.ToDto(),
            State = workOrder.State,
            Spot = workOrder.Spot,
            RepairTasks = workOrder.RepairTasks.Select(rt => rt.Name).ToList(),
            Labor = workOrder.Labor is null ? null : $"{workOrder.Labor.FirstName} {workOrder.Labor.LastName}",
            InvoiceId = workOrder.Invoice?.Id,
            EndAtUtc= workOrder.EndAtUtc,
            StartAtUtc= workOrder.StartAtUtc,
            Customer = workOrder.Vehicle?.Customer is null ? null : workOrder.Vehicle.Customer.Name
        };
    }
}
