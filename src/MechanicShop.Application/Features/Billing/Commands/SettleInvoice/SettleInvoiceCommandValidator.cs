using FluentValidation;

namespace MechanicShop.Application.Features.Billing.Commands.SettleInvoice;

public sealed class SettleInvoiceCommandValidator 
    : AbstractValidator<SettleInvoiceCommand>
{
    public SettleInvoiceCommandValidator()
    {
        RuleFor(request => request.InvoiceId)
            .NotEmpty()
            .WithErrorCode("InvoiceId.IsRequired")
            .WithMessage("InvoiceId is required.");
    }
}
