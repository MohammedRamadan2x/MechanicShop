using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;

public sealed class AssignLaborCommandValidator 
    : AbstractValidator<AssignLaborCommand>
{
    public AssignLaborCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
         .NotEmpty()
         .WithErrorCode("WorkOrderId.IsRequired")
         .WithMessage("WorkOrderId is required.");

        RuleFor(x => x.LaborId)
           .NotEmpty()
           .WithErrorCode("LaborId.IsRequired")
           .WithMessage("LaborId is required.");
    }
}