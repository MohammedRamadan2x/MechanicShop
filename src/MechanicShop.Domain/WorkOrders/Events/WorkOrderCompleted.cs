using MechanicShop.Domain.Common;

namespace MechanicShop.Domain.WorkOrders.Events;

public sealed class WorkOrderCompleted(Guid workOrderId) : DomainEvent
{
    public Guid WorkOrderId { get; } = workOrderId;
}

//public sealed class WorkOrderCompleted : DomainEvent
//{
//    public Guid WorkOrderId { get; set; }
//}