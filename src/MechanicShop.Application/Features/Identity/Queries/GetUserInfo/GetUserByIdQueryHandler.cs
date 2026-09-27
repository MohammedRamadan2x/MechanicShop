using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserInfo;

public class GetUserByIdQueryHandler(
    ILogger<GetUserByIdQueryHandler> logger, 
    IIdentityService identityService)
    : IRequestHandler<GetUserByIdQuery, Result<AppUserDto>>
{
    private readonly ILogger<GetUserByIdQueryHandler> _logger = logger;
    private readonly IIdentityService _identityService = identityService;

    public async Task<Result<AppUserDto>> Handle(
        GetUserByIdQuery query, 
        CancellationToken ct)
    {
        var getUserByIdResult = await _identityService
            .GetUserByIdAsync(query.UserId!);

        if (getUserByIdResult.IsFailure)
        {
            _logger.LogWarning(
                "Failed to retrieve user with Id {UserId}. " +
                "Error: {ErrorDescription}",
                query.UserId,
                getUserByIdResult.TopError.Description);

            return getUserByIdResult.Errors;
        }

        _logger.LogInformation(
            "User with Id {UserId} retrieved successfully.",
            query.UserId);

        return getUserByIdResult.Value;
    }
}