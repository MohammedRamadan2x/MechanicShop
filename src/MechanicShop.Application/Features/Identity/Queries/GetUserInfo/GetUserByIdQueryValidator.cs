using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserInfo;

public sealed class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode("UserId.IsRequired")
            .WithMessage("User ID is required.");
    }
}