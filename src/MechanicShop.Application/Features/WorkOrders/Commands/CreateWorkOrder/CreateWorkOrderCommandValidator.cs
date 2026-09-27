using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderCommandValidator 
    : AbstractValidator<CreateWorkOrderCommand>
{
    public CreateWorkOrderCommandValidator()
    {
        RuleFor(request => request.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required.");

        RuleFor(request => request.StartAt)
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("StartAt must be in the future.");

        RuleFor(request => request.RepairTaskIds)
            .NotEmpty()
            .WithMessage("At least one repair task must be selected.");

        RuleFor(request => request.LaborId)
            .Must(laborId => laborId is null || laborId != Guid.Empty)
            .WithMessage("If provided, LaborId must not be empty.");

        RuleFor(request => request.Spot)
          .IsInEnum()
          .WithErrorCode("Spot.Invalid")
          .WithMessage("Spot must be a valid Spot value. [A, B, C, D]");

        RuleFor(request => request.RepairTaskIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithErrorCode("RepairTasks.Duplicated")
            .WithMessage("RepairTaskIds must not contain duplicate values.");
    }
}