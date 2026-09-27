using FluentValidation;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;

public sealed class UpdateRepairTaskPartCommandValidator 
    : AbstractValidator<UpdateRepairTaskPartCommand>
{
    public UpdateRepairTaskPartCommandValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters.");

        RuleFor(p => p.Cost)
            .InclusiveBetween(1, 10_000)
            .WithMessage("Cost must be between 1 and 10,000.");

        RuleFor(p => p.Quantity)
            .InclusiveBetween(1, 10)
            .WithMessage("Quantity must be between 1 and 10.");
    }
}