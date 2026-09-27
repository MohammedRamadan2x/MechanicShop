using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Tests.Common.Customers;

using Xunit;

namespace MechanicShop.Domain.UnitTests.Customers;

public class VehicleTests
{
    [Fact]
    public void CreateVehicle_ShouldSucceed_WithValidData()
    {
        var id = Guid.NewGuid();
        const string make = "Honda";
        const string model = "Accord";
        const int year = 2024;
        const string licensePlate = "ABC 123";

        var result = VehicleFactory.CreateVehicle(
            id, 
            make, 
            model, 
            year, 
            licensePlate);

        Assert.True(result.IsSuccess);

        var vehicle = result.Value;

        Assert.Equal(id, vehicle.Id);
        Assert.Equal(make, vehicle.Make);
        Assert.Equal(model, vehicle.Model);
        Assert.Equal(year, vehicle.Year);
        Assert.Equal(licensePlate, vehicle.LicensePlate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateVehicle_ShouldFail_WhenInvalidMake(string invaldiMake)
    {
        var result = VehicleFactory.CreateVehicle(make: invaldiMake);

        Assert.True(result.IsFailure);

        Assert.Equal(VehicleErrors.MakeRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateVehicle_ShouldFail_WhenInvalidModel(string invalidModel)
    {
        var result = VehicleFactory.CreateVehicle(model: invalidModel);

        Assert.True(result.IsFailure);

        Assert.Equal(VehicleErrors.ModelRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateVehicle_ShouldFail_WhenInvalidLicensePlate(string invalidPlate)
    {
        var result = VehicleFactory.CreateVehicle(licensePlate: invalidPlate);

        Assert.True(result.IsFailure);

        Assert.Equal(VehicleErrors.LicensePlateRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(1700)]
    [InlineData(3000)]
    public void CreateVehicle_ShouldFail_WhenInvalidYear(int invalidYear)
    {
        var result = VehicleFactory.CreateVehicle(year: invalidYear);

        Assert.True(result.IsFailure);

        Assert.Equal(VehicleErrors.InvalidYear.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateVehicle_ShouldSucceed_WithValidData()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        var result = vehicle.Update("Toyota", "Camry", 2022, "XYZ 789");

        Assert.True(result.IsSuccess);
        Assert.Equal("Toyota", vehicle.Make);
        Assert.Equal("Camry", vehicle.Model);
        Assert.Equal(2022, vehicle.Year);
        Assert.Equal("XYZ 789", vehicle.LicensePlate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateVehicle_ShouldFail_WhenInvalidMake(string invaldiMake)
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        var result = vehicle.Update(invaldiMake, "Model", 2022, "XYZ123");

        Assert.True(result.IsFailure);

        Assert.Equal(VehicleErrors.MakeRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateVehicle_ShouldFail_WhenInvalidModel(string invalidModel)
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        var result = vehicle.Update("Make", invalidModel, 2022, "XYZ123");

        Assert.True(result.IsFailure);

        Assert.Equal(VehicleErrors.ModelRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateVehicle_ShouldFail_WhenInvalidLicensePlate(string invalidPlate)
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        var result = vehicle.Update("Make", "Model", 2022, invalidPlate);

        Assert.True(result.IsFailure);

        Assert.Equal(VehicleErrors.LicensePlateRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(1800)]
    [InlineData(5000)]
    public void UpdateVehicle_ShouldFail_WhenInvalidYear(int year)
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        var result = vehicle.Update("Make", "Model", year, "XYZ123");

        Assert.True(result.IsFailure);

        Assert.Equal(VehicleErrors.InvalidYear.Code, result.TopError.Code);
    }

    [Fact]
    public void VehicleInfo_ShouldReturnFormattedString()
    {
        var vehicle = VehicleFactory.CreateVehicle(
            make: "Ford", 
            model: "Mustang", 
            year: 2021).Value;

        Assert.Equal("Ford | Mustang | 2021", vehicle.VehicleInfo);
    }
}