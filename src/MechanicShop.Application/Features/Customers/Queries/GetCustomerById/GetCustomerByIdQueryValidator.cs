using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryValidator 
    : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(request => request.CustomerId)
            .NotEmpty()
            .WithErrorCode("CustomerId.IsRequired")
            .WithMessage("CustomerId is required.");
    }
}