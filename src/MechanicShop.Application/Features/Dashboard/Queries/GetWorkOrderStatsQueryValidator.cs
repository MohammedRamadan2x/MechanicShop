using FluentValidation;

namespace MechanicShop.Application.Features.Dashboard.Queries;

public sealed class GetWorkOrderStatsQueryValidator 
    : AbstractValidator<GetWorkOrderStatsQuery>
{
    public GetWorkOrderStatsQueryValidator()
    {
        RuleFor(request => request.Date)
            .NotEmpty()
            .WithErrorCode("Date.IsRequired")
            .WithMessage("Date is required.");
    }
}