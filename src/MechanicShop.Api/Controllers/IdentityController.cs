using Asp.Versioning;

using MechanicShop.Application.Features.Identity.Queries.GenerateToken;
using MechanicShop.Application.Features.Identity.Queries.RefreshToken;
using MechanicShop.Application.Features.Identity.Queries.GetUserInfo;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;
using MechanicShop.Contracts.Requests.Identity;

namespace MechanicShop.Api.Controllers;


[Route("identity")]
[ApiVersionNeutral]
public sealed class IdentityController(ISender sender) : ApiController
{
    [HttpPost("token/generate")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Generates an access and refresh token for a valid user.")]
    [EndpointDescription("Authenticates a user using provided credentials and returns a JWT token pair.")]
    [EndpointName("GenerateToken")]
    public async Task<IActionResult> GenerateToken(
        [FromBody] GenerateTokenRequest request, 
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GenerateTokenQuery(
                request.Email, 
                request.Password), 
            ct);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpPost("token/refresh-token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Refreshes access token using a valid refresh token.")]
    [EndpointDescription("Exchanges an expired access token and a valid refresh token for a new token pair.")]
    [EndpointName("RefreshToken")]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request, 
        CancellationToken ct)
    {
        var result = await sender.Send(
            new RefreshTokenQuery(
                request.RefreshToken, 
                request.ExpiredAccessToken), 
            ct);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpGet("current-user/claims")]
    [Authorize]
    [ProducesResponseType(typeof(AppUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Gets the current authenticated user's info.")]
    [EndpointDescription("Returns user information for the currently authenticated user based on the access token.")]
    [EndpointName("GetCurrentUser")]
    public async Task<IActionResult> GetCurrentUserInfo(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await sender.Send(new GetUserByIdQuery(userId), ct);

        return result.Match(response => Ok(response), Problem);
    }
}