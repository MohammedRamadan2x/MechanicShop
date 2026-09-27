using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;

namespace MechanicShop.Infrastructure.Identity;

public class TokenProvider(
    IConfiguration configuration,
    IAppDbContext context)
    : ITokenProvider
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IAppDbContext _context = context;

    public async Task<Result<TokenResponse>> GenerateJwtTokenAsync(
        AppUserDto user, 
        CancellationToken ct = default)
    {
        var createTokenResult = await CreateAsync(user, ct);

        if (createTokenResult.IsFailure)
        {
            return createTokenResult.Errors;
        }

        return createTokenResult.Value;
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!)),
            ValidateIssuer = true,
            ValidIssuer = _configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = _configuration["JwtSettings:Audience"],
            ValidateLifetime = false, // Ignore token expiration
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(
            token, 
            tokenValidationParameters, 
            out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256, 
                StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token.");
        }

        return principal;
    }

    private async Task<Result<TokenResponse>> CreateAsync(
        AppUserDto user,
        CancellationToken ct = default)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;
        var key = jwtSettings["Secret"]!;

        var expires = DateTime.UtcNow.AddMinutes(
            int.Parse(jwtSettings["TokenExpirationInMinutes"]!));

        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.UserId!),
            new (JwtRegisteredClaimNames.Email, user.Email!)
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new (ClaimTypes.Role, role));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = issuer,
            Audience = audience,
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var securityToken = tokenHandler.CreateToken(descriptor);

        var oldRefreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.UserId)
            .ExecuteDeleteAsync(ct);

        var createRefreshTokenResult = RefreshToken.Create(
            Guid.NewGuid(),
            GenerateRefreshToken(),
            user.UserId,
            DateTime.UtcNow.AddDays(7));

        if (createRefreshTokenResult.IsFailure)
        {
            return createRefreshTokenResult.Errors;
        }

        var refreshToken = createRefreshTokenResult.Value;

        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync(ct);

        return new TokenResponse
        {
            AccessToken = tokenHandler.WriteToken(securityToken),
            RefreshToken = refreshToken.Token,
            ExpiresOnUtc = expires
        };
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}