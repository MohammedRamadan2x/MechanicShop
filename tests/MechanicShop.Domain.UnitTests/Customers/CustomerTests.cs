using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;

using Xunit;

namespace MechanicShop.Domain.UnitTests.Customers;

public class CustomerTests
{
    [Fact]
    public void CreateCustomer_ShouldSucceed_WithValidData()
    {
        var id = Guid.NewGuid();
        const string name = "Customer #1";
        const string phoneNumber = "5555555555";
        const string email = "customer01@localhost";
        List<Vehicle> vehicles = [VehicleFactory.CreateVehicle().Value];

        var result = CustomerFactory.CreateCustomer(
            id, 
            name, 
            phoneNumber, 
            email, 
            vehicles);

        Assert.True(result.IsSuccess);

        var customer = result.Value;

        Assert.IsType<Customer>(customer);
        Assert.NotNull(customer);
        Assert.Equal(id, customer.Id);
        Assert.Equal(name, customer.Name);
        Assert.Equal(phoneNumber, customer.PhoneNumber);
        Assert.Equal(email, customer.Email);
        Assert.Single(customer.Vehicles);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateCustomer_ShouldFail_WhenNameEmptyOrNull(string invalidName)
    {
        var result = CustomerFactory.CreateCustomer(name: invalidName);

        Assert.True(result.IsFailure);

        Assert.Equal(CustomerErrors.NameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateCustomer_ShouldFail_WhenPhoneNumberEmptyOrNull(
        string invalidPhone)
    {
        var result = CustomerFactory.CreateCustomer(phoneNumber: invalidPhone);

        Assert.True(result.IsFailure);

        Assert.Equal(CustomerErrors.PhoneNumberRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("123")] // less than 7
    [InlineData("12345678910111213")] // greater than 15
    public void CreateCustomer_ShouldFail_WhenInvalidPhoneNumber(string invalidPhone)
    {
        var result = CustomerFactory.CreateCustomer(phoneNumber: invalidPhone);

        Assert.True(result.IsFailure);

        Assert.Equal(CustomerErrors.InvalidPhoneNumber.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateCustomer_ShouldFail_WhenEmailEmptyOrNull(string invalidEmail)
    {
        var result = CustomerFactory.CreateCustomer(email: invalidEmail);

        Assert.True(result.IsFailure);

        Assert.Equal(CustomerErrors.EmailRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("abc1.@")]
    public void CreateCustomer_ShouldFail_WhenInvalidEmail(string invalidEmail)
    {
        var result = CustomerFactory.CreateCustomer(email: invalidEmail);

        Assert.True(result.IsFailure);

        Assert.Equal(CustomerErrors.InvalidEmail.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateCustomer_ShouldSucceed_WithValidData()
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var result = customer.Update(
            "Updated Name", 
            "updated@email.com", 
            "1234567890");

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);
    }

    [Fact]
    public void UpdateCustomer_ShouldFail_WhenNameEmptyOrNull()
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var result = customer.Update(
            string.Empty, 
            "newEmail@localhost", 
            "123-1232");

        Assert.True(result.IsFailure);

        Assert.Equal(CustomerErrors.NameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("123")] // less than 7
    [InlineData("12345678910111213")] // greater than 15
    public void UpdateCustomer_ShouldFail_WhenInvalidPhoneNumber(
        string invalidPhone)
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var result = customer.Update(
            "New name", 
            "newEmail@localhost", 
            invalidPhone);

        Assert.True(result.IsFailure);

        Assert.Equal(CustomerErrors.InvalidPhoneNumber.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateCustomer_ShouldFail_WhenPhoneNumberEmptyOrNull(
        string invalidPhone)
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var result = customer.Update(
            "New name",
            "newEmail@localhost",
            invalidPhone);

        Assert.True(result.IsFailure);

        Assert.Equal(CustomerErrors.PhoneNumberRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateCustomer_ShouldFail_WhenEmailEmptyOrNull(string invalidEmail)
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var result = customer.Update("New name", invalidEmail, "1234567890");

        Assert.True(result.IsFailure);

        Assert.Equal(CustomerErrors.EmailRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("abc1.@")]
    public void UpdateCustomer_ShouldFail_WhenInvalidEmail(string invalidEmail)
    {
        var customer = CustomerFactory.CreateCustomer().Value;

        var result = customer.Update("New name", invalidEmail, "1234567890");

        Assert.True(result.IsFailure);

        Assert.Equal(CustomerErrors.InvalidEmail.Code, result.TopError.Code);
    }

    [Fact]
    public void UpsertVehicles_ShouldAddNewVehiclesAndUpdateExisting()
    {
        var originalVehicle = VehicleFactory.CreateVehicle(make: "Ford").Value;

        var customer = CustomerFactory.CreateCustomer(
            vehicles: [originalVehicle]).Value;

        var updatedVehicle = VehicleFactory.CreateVehicle(
            id: originalVehicle.Id, 
            make: "UpdatedFord").Value;

        var newVehicle = VehicleFactory.CreateVehicle(make: "NewBrand").Value;

        var result = customer.UpsertVehicles([updatedVehicle, newVehicle]);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, customer.Vehicles.Count());
        Assert.Equal(Result.Updated, result.Value);
        Assert.Contains(customer.Vehicles, 
            v => v.Id == updatedVehicle.Id && v.Make == "UpdatedFord");
        Assert.Contains(customer.Vehicles, 
            v => v.Id == newVehicle.Id && v.Make == "NewBrand");
    }

    [Fact]
    public void UpsertVehicles_ShouldRemoveVehiclesNotInIncomingList()
    {
        var existing1 = VehicleFactory.CreateVehicle().Value;
        var existing2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(
            vehicles: [existing1, existing2]).Value;

        var incoming = VehicleFactory.CreateVehicle(id: existing2.Id).Value;

        var result = customer.UpsertVehicles([incoming]);

        Assert.Equal(Result.Updated, result.Value);
        Assert.True(result.IsSuccess);
        Assert.Single(customer.Vehicles);
        Assert.Equal(existing2.Id, customer.Vehicles.Single().Id);
    }
}