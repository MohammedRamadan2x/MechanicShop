using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;

public sealed class DeleteWorkOrderCommandValidator 
    : AbstractValidator<DeleteWorkOrderCommand>
{
    public DeleteWorkOrderCommandValidator()
    {
        RuleFor(request => request.WorkOrderId)
           .NotEmpty()
           .WithErrorCode("WorkOrderId.IsRequired")
           .WithMessage("WorkOrderId is required.");
    }
}