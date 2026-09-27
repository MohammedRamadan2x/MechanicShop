using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public sealed class UpdateVehicleCommandValidator 
    : AbstractValidator<UpdateVehicleCommand>
{
    public UpdateVehicleCommandValidator()
    {
        RuleFor(x => x.Make)
            .NotEmpty()
            .WithMessage("Make is required.")
            .MaximumLength(50)
            .WithMessage("Make cannot exceed 50 characters.");

        RuleFor(x => x.Model)
            .NotEmpty()
            .WithMessage("Model is required.")
            .MaximumLength(50)
            .WithMessage("Model cannot exceed 50 characters.");

        RuleFor(x => x.LicensePlate)
            .NotEmpty()
            .WithMessage("License plate is required.")
            .MaximumLength(10)
            .WithMessage("License plate cannot exceed 10 characters.");
    }
}