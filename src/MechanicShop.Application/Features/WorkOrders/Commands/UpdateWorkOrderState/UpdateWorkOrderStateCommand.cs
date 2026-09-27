using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;

public sealed record UpdateWorkOrderStateCommand(
    Guid WorkOrderId,
    WorkOrderState State) : IRequest<Result<Updated>>;