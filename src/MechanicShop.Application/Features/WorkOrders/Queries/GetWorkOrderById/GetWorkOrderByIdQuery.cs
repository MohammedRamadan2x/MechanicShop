using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;

public sealed record GetWorkOrderByIdQuery(Guid WorkOrderId) 
    : ICachedQuery<Result<WorkOrderDto>>
{
    public string CacheKey => $"work-order:{WorkOrderId}";

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
    
    public string[] Tags => ["work-order"];
}