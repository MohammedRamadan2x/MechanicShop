namespace MechanicShop.Application.Features.Customers.Dtos;

public sealed class CustomerDto
{
    public Guid CustomerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public List<VehicleDto> Vehicles { get; init; } = [];
}