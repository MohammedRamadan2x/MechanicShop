using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Identity;

public sealed class RefreshToken : AuditableEntity
{
    public string Token { get; }
    public string UserId { get; }
    public DateTimeOffset ExpiresAtUtc { get; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private RefreshToken()
    { }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private RefreshToken(
        Guid id, 
        string token, 
        string userId, 
        DateTimeOffset expiresAtUtc) : base(id)
    {
        Token = token;
        UserId = userId;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static Result<RefreshToken> Create(
        Guid id, 
        string token, 
        string userId, 
        DateTimeOffset expiresAtUtc)
    {
        if (id == Guid.Empty)
        {
            return RefreshTokenErrors.IdRequired;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return RefreshTokenErrors.TokenRequired;
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return RefreshTokenErrors.UserIdRequired;
        }

        if (expiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return RefreshTokenErrors.InvalidExpiry;
        }

        return new RefreshToken(id, token.Trim(), userId.Trim(), expiresAtUtc);
    }
}