using FluentValidation;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;

public sealed class GetInvoicePdfQueryValidator 
    : AbstractValidator<GetInvoicePdfQuery>
{
    public GetInvoicePdfQueryValidator()
    {
        RuleFor(request => request.InvoiceId)
            .NotEmpty()
            .WithErrorCode("InvoiceId.IsRequired")
            .WithMessage("InvoiceId is required.");
    }
}