using FluentValidation;

namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice;

public sealed class IssueInvoiceCommandValidator
    : AbstractValidator<IssueInvoiceCommand>
{
    public IssueInvoiceCommandValidator()
    {
        RuleFor(request => request.WorkOrderId)
            .NotEmpty()
            .WithErrorCode("WorkOrderId.IsRequired")
            .WithMessage("WorkOrderId is required.");
    }
}
