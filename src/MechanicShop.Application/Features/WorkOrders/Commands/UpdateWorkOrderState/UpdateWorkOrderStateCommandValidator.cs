using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState;

public sealed class UpdateWorkOrderStateCommandValidator 
    : AbstractValidator<UpdateWorkOrderStateCommand>
{
    public UpdateWorkOrderStateCommandValidator()
    {
        RuleFor(request => request.State)
           .IsInEnum()
           .WithErrorCode("WorkOrderStatus.Invalid")
           .WithMessage("Status must be a valid WorkOrderStatus value.");
    }
}
