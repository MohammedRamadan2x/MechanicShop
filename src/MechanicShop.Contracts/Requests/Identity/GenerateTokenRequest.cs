using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.Identity;

public class GenerateTokenRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email is invalid.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}