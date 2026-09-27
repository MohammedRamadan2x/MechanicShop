using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById;

public sealed class GetWorkOrderByIdQueryValidator 
    : AbstractValidator<GetWorkOrderByIdQuery>
{
    public GetWorkOrderByIdQueryValidator()
    {
        RuleFor(request => request.WorkOrderId)
            .NotEmpty()
            .WithErrorCode("WorkOrderId.IsRequired")
            .WithMessage("WorkOrderId is required.");
    }
}