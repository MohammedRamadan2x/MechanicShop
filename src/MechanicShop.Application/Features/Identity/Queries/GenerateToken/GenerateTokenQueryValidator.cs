using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Queries.GenerateToken;

public sealed class GenerateTokenQueryValidator 
    : AbstractValidator<GenerateTokenQuery>
{
    public GenerateTokenQueryValidator()
    {
        RuleFor(request => request.Email)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("Email.NullOrEmpty")
            .WithMessage("Email cannot be null or empty.");

        RuleFor(request => request.Password)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("Password.NullOrEmpty")
            .WithMessage("Password cannot be null or empty.");
    }
}