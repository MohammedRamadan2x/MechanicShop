namespace MechanicShop.Client.Models;

public class CustomerModel
{
    public Guid CustomerId { get; set; }
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public List<VehicleModel> Vehicles { get; set; } = [];
}