using MechanicShop.Application.Features.Customers.Mappers;

using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Domain.Customers;

using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class CustomerMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        // Arrange
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicles = customer.Vehicles.ToList();

        // Act
        var dto = customer.ToDto();

        // Assert
        Assert.Equal(customer.Id, dto.CustomerId);
        Assert.Equal(customer.Name, dto.Name);
        Assert.Equal(customer.Email, dto.Email);
        Assert.Equal(customer.PhoneNumber, dto.PhoneNumber);

        Assert.NotNull(dto.Vehicles);
        Assert.Equal(vehicles.Count, dto.Vehicles.Count);

        for (int i = 0; i < vehicles.Count; i++)
        {
            var vehicle = vehicles[i];
            var vehicleDto = dto.Vehicles[i];

            Assert.Equal(vehicle.Id, vehicleDto.VehicleId);
            Assert.Equal(vehicle.Make, vehicleDto.Make);
            Assert.Equal(vehicle.Model, vehicleDto.Model);
            Assert.Equal(vehicle.Year, vehicleDto.Year);
            Assert.Equal(vehicle.LicensePlate, vehicleDto.LicensePlate);
        }
    }

    [Fact]
    public void ToDtos_ShouldMapListCorrectly()
    {
        // Arrange
        var customer1 = CustomerFactory.CreateCustomer().Value;
        var customer2 = CustomerFactory.CreateCustomer().Value;

        var customers = new List<Customer>
        {
            customer1,
            customer2
        };

        // Act
        var dtos = customers.ToDtos();

        // Assert
        Assert.Equal(customers.Count, dtos.Count);

        Assert.Equal(customer1.Id, dtos[0].CustomerId);
        Assert.Equal(customer1.Name, dtos[0].Name);
        Assert.Equal(customer1.Email, dtos[0].Email);
        Assert.Equal(customer1.PhoneNumber, dtos[0].PhoneNumber);

        Assert.Equal(customer2.Id, dtos[1].CustomerId);
        Assert.Equal(customer2.Name, dtos[1].Name);
        Assert.Equal(customer2.Email, dtos[1].Email);
        Assert.Equal(customer2.PhoneNumber, dtos[1].PhoneNumber);
    }

    [Fact]
    public void VehicleToDto_ShouldMapCorrectly()
    {
        // Arrange
        var vehicle = VehicleFactory.CreateVehicle().Value;

        // Act
        var dto = vehicle.ToDto();

        // Assert
        Assert.Equal(vehicle.Id, dto.VehicleId);
        Assert.Equal(vehicle.Make, dto.Make);
        Assert.Equal(vehicle.Model, dto.Model);
        Assert.Equal(vehicle.Year, dto.Year);
        Assert.Equal(vehicle.LicensePlate, dto.LicensePlate);
    }

    [Fact]
    public void VehicleToDtos_ShouldMapListCorrectly()
    {
        // Arrange
        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;

        var vehicles = new List<Vehicle>
        {
            vehicle1,
            vehicle2
        };

        // Act
        var dtos = vehicles.ToDtos();

        // Assert
        Assert.Equal(vehicles.Count, dtos.Count);

        Assert.Equal(vehicle1.Id, dtos[0].VehicleId);
        Assert.Equal(vehicle1.Make, dtos[0].Make);
        Assert.Equal(vehicle1.Model, dtos[0].Model);
        Assert.Equal(vehicle1.Year, dtos[0].Year);
        Assert.Equal(vehicle1.LicensePlate, dtos[0].LicensePlate);

        Assert.Equal(vehicle2.Id, dtos[1].VehicleId);
        Assert.Equal(vehicle2.Make, dtos[1].Make);
        Assert.Equal(vehicle2.Model, dtos[1].Model);
        Assert.Equal(vehicle2.Year, dtos[1].Year);
        Assert.Equal(vehicle2.LicensePlate, dtos[1].LicensePlate);
    }

    [Fact]
    public void ToDto_ShouldThrowArgumentNullException_WhenCustomerIsNull()
    {
        // Arrange
        Customer customer = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => customer.ToDto());
    }

    [Fact]
    public void VehicleToDto_ShouldThrowArgumentNullException_WhenVehicleIsNull()
    {
        // Arrange
        Vehicle vehicle = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => vehicle.ToDto());
    }
}