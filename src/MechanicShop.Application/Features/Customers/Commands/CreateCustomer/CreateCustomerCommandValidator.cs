using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandValidator
    : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters.");

        RuleFor(c => c.Email)
            .EmailAddress()
            .WithMessage("Invalid email.")
            .MaximumLength(100)
            .WithMessage("Email cannot exceed 100 characters.");

        RuleFor(c => c.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required.")
            .Matches(@"^\+?\d{7,15}$")
            .WithMessage("Phone number must be 7-15 digits and may start with '+'.");

        RuleFor(c => c.Vehicles)
            .NotNull()
            .WithMessage("Vehicles list cannot be null.")
            .Must(v => v.Count > 0)
            .WithMessage("At least one vehicle is required.");

        RuleForEach(c => c.Vehicles)
            .SetValidator(new CreateVehicleCommandValidator());
    }
}