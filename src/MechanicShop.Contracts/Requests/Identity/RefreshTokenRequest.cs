using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.Identity;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token is required.")]
    public string RefreshToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "Expired access token is required.")]
    public string ExpiredAccessToken { get; set; } = string.Empty;
}