using FluentValidation;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

public sealed class UpdateWorkOrderRepairTasksCommandValidator 
    : AbstractValidator<UpdateWorkOrderRepairTasksCommand>
{
    public UpdateWorkOrderRepairTasksCommandValidator()
    {
        RuleFor(request => request.WorkOrderId)
           .NotEmpty()
           .WithErrorCode("WorkOrderId.IsRequired")
           .WithMessage("WorkOrderId is required.");

        RuleFor(request => request.RepairTaskIds)
          .NotEmpty()
          .WithErrorCode("RepairTasks.IsRequired")
          .WithMessage("At least one repair task must be provided.");

        RuleFor(request => request.RepairTaskIds)
            .Must(ids => ids.Distinct().Count() == ids.Length)
            .WithErrorCode("RepairTasks.Duplicated")
            .WithMessage("RepairTaskIds must not contain duplicate values.");
    }
}