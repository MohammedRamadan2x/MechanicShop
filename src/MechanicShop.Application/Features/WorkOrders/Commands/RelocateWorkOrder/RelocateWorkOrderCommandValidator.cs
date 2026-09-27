using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;

public sealed class RelocateWorkOrderCommandValidator
    : AbstractValidator<RelocateWorkOrderCommand>
{
    public RelocateWorkOrderCommandValidator()
    {
        RuleFor(request => request.WorkOrderId)
            .NotEmpty()
            .WithMessage("WorkOrderId is required.");

        RuleFor(request => request.NewStartAt)
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("New start time must be in the future.");

        RuleFor(request => request.NewSpot)
            .IsInEnum()
            .WithMessage("The selected spot is invalid.");
    }
}