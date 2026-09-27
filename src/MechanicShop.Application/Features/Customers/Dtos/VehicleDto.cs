namespace MechanicShop.Application.Features.Customers.Dtos;

public sealed record VehicleDto(
    Guid VehicleId, 
    string Make, 
    string Model, 
    int Year, 
    string LicensePlate);