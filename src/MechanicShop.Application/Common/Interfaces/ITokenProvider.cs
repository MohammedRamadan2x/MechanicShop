using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Features.Identity;
using MechanicShop.Domain.Common.Results;
using System.Security.Claims;

namespace MechanicShop.Application.Common.Interfaces;

public interface ITokenProvider
{
    Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, 
        CancellationToken ct = default);

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}