using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public sealed record CreateWorkOrderCommand(
    Spot Spot,
    Guid VehicleId,
    DateTimeOffset StartAt,
    List<Guid> RepairTaskIds,
    Guid? LaborId)
    : IRequest<Result<WorkOrderDto>>;
