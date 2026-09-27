using FluentValidation;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;

public sealed class GetInvoiceByIdQueryValidator 
    : AbstractValidator<GetInvoiceByIdQuery>
{
    public GetInvoiceByIdQueryValidator()
    {
        RuleFor(request => request.InvoiceId)
            .NotEmpty()
            .WithErrorCode("InvoiceId.IsRequired")
            .WithMessage("InvoiceId is required.");

    }
}
