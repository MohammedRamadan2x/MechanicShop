using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;

namespace MechanicShop.Tests.Common.Auth;

public static class RefreshTokenFactory
{
    public static Result<RefreshToken> CreateRefreshToken(
        Guid? id = null, 
        string? token = null, 
        string? userId = null, 
        DateTimeOffset? expiresOnUtc = null)
    {
        return RefreshToken.Create(
            id ?? Guid.NewGuid(),
            token ?? "sometoken",
            userId ?? Guid.NewGuid().ToString(),
            expiresOnUtc ?? DateTime.UtcNow.AddDays(7));
    }
}
