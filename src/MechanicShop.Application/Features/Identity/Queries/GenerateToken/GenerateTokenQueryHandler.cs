using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GenerateToken;

public class GenerateTokenQueryHandler(
    ILogger<GenerateTokenQueryHandler> logger, 
    IIdentityService identityService, 
    ITokenProvider tokenProvider)
    : IRequestHandler<GenerateTokenQuery, Result<TokenResponse>>
{
    private readonly ILogger<GenerateTokenQueryHandler> _logger = logger;
    private readonly IIdentityService _identityService = identityService;
    private readonly ITokenProvider _tokenProvider = tokenProvider;

    public async Task<Result<TokenResponse>> Handle(
        GenerateTokenQuery query, 
        CancellationToken ct)
    {
        var userResponse = await _identityService
            .AuthenticateAsync(query.Email, query.Password);

        if (userResponse.IsFailure)
        {
            _logger.LogWarning(
                "Authentication failed for email: {Email}. " +
                "Error: {ErrorDescription}",
                query.Email,
                userResponse.TopError.Description);

            return userResponse.Errors;
        }

        var generateTokenResult = await _tokenProvider
            .GenerateJwtTokenAsync(userResponse.Value, ct);

        if (generateTokenResult.IsFailure)
        {
            _logger.LogError(
                "Failed to generate JWT token for user {Email}. " +
                "Error: {ErrorDescription}",
                query.Email,
                generateTokenResult.TopError.Description);

            return generateTokenResult.Errors;
        }

        _logger.LogInformation(
            "JWT token generated successfully for user {Email}.",
            query.Email);

        return generateTokenResult.Value;
    }
}