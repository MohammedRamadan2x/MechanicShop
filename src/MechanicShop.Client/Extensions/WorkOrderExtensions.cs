using MechanicShop.Client.Models;

namespace MechanicShop.Client.Extensions;

public static class WorkOrderExtensions
{
    public static void AdjustTimeToLocal(this WorkOrderModel WorkOrder)
    {
        ArgumentNullException.ThrowIfNull(WorkOrder);

        WorkOrder.StartAtUtc = WorkOrder.StartAtUtc.ToLocalTime();
        WorkOrder.EndAtUtc = WorkOrder.EndAtUtc.ToLocalTime();
    }

    public static void AdjustTimeToLocal(this WorkOrderListItemModel WorkOrder)
    {
        ArgumentNullException.ThrowIfNull(WorkOrder);

        WorkOrder.StartAtUtc = WorkOrder.StartAtUtc.ToLocalTime();
        WorkOrder.EndAtUtc = WorkOrder.EndAtUtc.ToLocalTime();
    }
}