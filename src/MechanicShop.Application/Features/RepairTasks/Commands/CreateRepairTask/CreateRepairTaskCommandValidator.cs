using FluentValidation;

namespace MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;

public sealed class CreateRepairTaskCommandValidator 
    : AbstractValidator<CreateRepairTaskCommand>
{
    public CreateRepairTaskCommandValidator()
    {
        RuleFor(rt => rt.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters.");

        RuleFor(rt => rt.LaborCost)
            .InclusiveBetween(1, 10_000)
            .WithMessage("Labor cost must be between 1 and 10,000.");

        RuleFor(rt => rt.EstimatedDurationInMins)
            .NotNull()
            .WithMessage("Estimated duration is required.")
            .IsInEnum()
            .WithMessage("Invalid duration selected.");

        RuleFor(rt => rt.Parts)
            .NotNull()
            .WithMessage("Parts list cannot be null.")
            .Must(p => p.Count > 0)
            .WithMessage("At least one part is required.");

        RuleFor(x => x.Parts)
            .Must(parts =>
                parts is not null &&
                parts.Select(p => p.Name.Trim())
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .Count() == parts.Count)
            .WithErrorCode("RepairTask.DuplicatePartName")
            .WithMessage("Duplicate part names are not allowed.");

        RuleForEach(rt => rt.Parts)
            .SetValidator(new CreateRepairTaskPartCommandValidator());
    }
}