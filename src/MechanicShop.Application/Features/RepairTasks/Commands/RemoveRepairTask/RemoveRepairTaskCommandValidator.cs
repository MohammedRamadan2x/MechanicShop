using FluentValidation;

namespace MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;

public class RemoveRepairTaskCommandValidator 
    : AbstractValidator<RemoveRepairTaskCommand>
{
    public RemoveRepairTaskCommandValidator()
    {
        RuleFor(rt => rt.RepairTaskId)
            .NotEmpty()
            .WithMessage("Repair task Id is required.");
    }
}