using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;

namespace MechanicShop.Tests.Common.Customers;

public static class CustomerFactory
{
    public static Result<Customer> CreateCustomer(
        Guid? id = null, 
        string? name = null, 
        string? phoneNumber = null, 
        string? email = null, 
        List<Vehicle>? vehicles = null)
    {
        return Customer.Create(
            id ?? Guid.NewGuid(),
            name ?? "Customer #1",
            phoneNumber ?? "5555555555",
            email ?? "customer01@localhost",
            vehicles ?? [VehicleFactory.CreateVehicle().Value, 
                         VehicleFactory.CreateVehicle().Value]);
    }
}