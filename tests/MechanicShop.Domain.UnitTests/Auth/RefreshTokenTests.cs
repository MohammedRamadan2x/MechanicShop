using MechanicShop.Tests.Common.Auth;
using MechanicShop.Domain.Identity;

using Xunit;

namespace MechanicShop.Domain.UnitTests.Auth;

public class RefreshTokenTests
{
    [Fact]
    public void CreateRefreshToken_ShouldSucceed_WithValidData()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        const string tokenValue = "token";
        var expiresOnUtc = DateTimeOffset.UtcNow.AddDays(7);

        var result = RefreshTokenFactory.CreateRefreshToken(
            id, 
            tokenValue, 
            userId, 
            expiresOnUtc);

        Assert.True(result.IsSuccess);

        var token = result.Value;

        Assert.NotNull(token);
        Assert.Equal(tokenValue, token.Token);
        Assert.False(string.IsNullOrWhiteSpace(token.UserId));
        Assert.Equal(userId, token.UserId);
        Assert.True(token.ExpiresAtUtc > DateTimeOffset.UtcNow);
    }

    [Fact]
    public void CreateRefreshToken_ShouldFail_WhenIdEmpty()
    {
        var result = RefreshTokenFactory.CreateRefreshToken(id: Guid.Empty);

        Assert.True(result.IsFailure);

        Assert.Equal(RefreshTokenErrors.IdRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateRefreshToken_ShouldFail_WhenInvalidToken(string? invalidToken)
    {
        var result = RefreshTokenFactory.CreateRefreshToken(token: invalidToken);

        Assert.True(result.IsFailure);

        Assert.Equal(RefreshTokenErrors.TokenRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateRefreshToken_ShouldFail_WhenInvalidUserId(string? invalidUserId)
    {
        var result = RefreshTokenFactory.CreateRefreshToken(userId: invalidUserId);

        Assert.True(result.IsFailure);

        Assert.Equal(RefreshTokenErrors.UserIdRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void CreateRefreshToken_ShouldFail_WhenExpiresOnUtcIsInPast()
    {
        var result = RefreshTokenFactory.CreateRefreshToken(
            expiresOnUtc: DateTimeOffset.UtcNow.AddMinutes(-1));

        Assert.True(result.IsFailure);

        Assert.Equal(RefreshTokenErrors.InvalidExpiry.Code, result.TopError.Code);
    }
}