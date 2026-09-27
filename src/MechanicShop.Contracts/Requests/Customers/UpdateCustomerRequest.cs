using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.Customers;

public class UpdateCustomerRequest
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(
        @"^\+?\d{7,15}$", 
        ErrorMessage = "Phone number must be 7–15 digits and may start with '+'.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email is invalid.")]
    public string Email { get; set; } = string.Empty;

    [MinLength(1, ErrorMessage = "At least one vehicle is required.")]
    public List<UpdateVehicleRequest> Vehicles { get; set; } = [];
}