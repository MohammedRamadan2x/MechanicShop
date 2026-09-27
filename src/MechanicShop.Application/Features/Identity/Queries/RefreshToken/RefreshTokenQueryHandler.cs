using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MechanicShop.Application.Features.Identity.Queries.RefreshToken;

public class RefreshTokenQueryHandler(
    ILogger<RefreshTokenQueryHandler> logger,
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<RefreshTokenQuery, Result<TokenResponse>>
{
    private readonly ILogger<RefreshTokenQueryHandler> _logger = logger;
    private readonly IIdentityService _identityService = identityService;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly IAppDbContext _context = context;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result<TokenResponse>> Handle(
        RefreshTokenQuery query, 
        CancellationToken ct)
    {
        var principal = _tokenProvider
            .GetPrincipalFromExpiredToken(query.ExpiredAccessToken);

        if (principal is null)
        {
            _logger.LogWarning("Expired access token is invalid");

            return ApplicationErrors.ExpiredAccessTokenInvalid;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            _logger.LogWarning(
                "Expired access token does not contain a valid user identifier.");

            return ApplicationErrors.UserIdClaimInvalid;
        }

        var getUserResult = await _identityService.GetUserByIdAsync(userId);

        if (getUserResult.IsFailure)
        {
            _logger.LogWarning(
                "Failed to retrieve user {UserId}. " +
                "Error: {ErrorDescription}",
                userId,
                getUserResult.TopError.Description);

            return getUserResult.Errors;
        }

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => 
            rt.Token == query.RefreshToken 
            &&
            rt.UserId == userId, 
            ct);

        if (refreshToken is null 
            || 
            refreshToken.ExpiresAtUtc < _timeProvider.GetUtcNow())
        {
            _logger.LogWarning(
                "Refresh token validation failed for user {UserId}. " +
                "The token is missing or expired.",
                userId);

            return ApplicationErrors.RefreshTokenExpired;
        }

        var generateTokenResult = await _tokenProvider
            .GenerateJwtTokenAsync(getUserResult.Value, ct);

        if (generateTokenResult.IsFailure)
        {
            _logger.LogError(
                "Failed to generate JWT token for user {UserId}. " +
                "Error: {ErrorDescription}",
                userId,
                generateTokenResult.TopError.Description);

            return generateTokenResult.Errors;
        }

        _logger.LogInformation(
            "JWT token refreshed successfully for user {UserId}.", 
            userId);

        return generateTokenResult.Value;
    }
}