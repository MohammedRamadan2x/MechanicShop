using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Identity;

public static class RefreshTokenErrors
{
    public static Error IdRequired =>
        Error.Validation("RefreshToken.IdRequired", "Refresh token ID is required.");

    public static Error TokenRequired =>
        Error.Validation("RefreshToken.TokenRequired", "Token value is required.");

    public static Error UserIdRequired =>
        Error.Validation("RefreshToken.UserIdRequired", "User ID is required.");

    public static Error InvalidExpiry =>
        Error.Validation("RefreshToken.InvalidExpiry", 
            "Refresh token expiration must be in the future.");
}