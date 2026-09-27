using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Queries.RefreshToken;

public sealed class RefreshTokenQueryValidator : AbstractValidator<RefreshTokenQuery>
{
    public RefreshTokenQueryValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.");

        RuleFor(x => x.ExpiredAccessToken)
            .NotEmpty()
            .WithMessage("Expired access token is required.");
    }
}