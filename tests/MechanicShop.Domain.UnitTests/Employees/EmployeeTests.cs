using MechanicShop.Tests.Common.Employees;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;

using Xunit;

namespace MechanicShop.Domain.UnitTests.Employees;
public class EmployeeTests
{
    [Fact]
    public void CreateEmployee_ShouldSucceed_WithValidData()
    {
        var id = Guid.NewGuid();
        const string firstName = "John";
        const string lastName = "Doe";
        const Role role = Role.Labor;

        var result = EmployeeFactory.CreateEmployee(
            id, 
            firstName, 
            lastName, 
            role);

        Assert.True(result.IsSuccess);

        var employee = result.Value;

        Assert.Equal(id, employee.Id);
        Assert.Equal(firstName, employee.FirstName);
        Assert.Equal(lastName, employee.LastName);
        Assert.Equal(role, employee.Role);
        Assert.Equal("John Doe", employee.FullName);
    }

    [Fact]
    public void CreateEmployee_ShouldFail_WhenIdEmpty()
    {
        var result = EmployeeFactory.CreateEmployee(id: Guid.Empty);

        Assert.True(result.IsFailure);

        Assert.Equal(EmployeeErrors.IdRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void CreateEmployee_ShouldFail_WhenFirstNameEmpty()
    {
        var result = EmployeeFactory.CreateEmployee(firstName: " ");

        Assert.True(result.IsFailure);

        Assert.Equal(EmployeeErrors.FirstNameRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void CreateEmployee_ShouldFail_WhenLastNameEmpty()
    {
        var result = EmployeeFactory.CreateEmployee(lastName: " ");

        Assert.True(result.IsFailure);

        Assert.Equal(EmployeeErrors.LastNameRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void CreateEmployee_ShouldFail_WhenInvalidRole()
    {
        var result = EmployeeFactory.CreateEmployee(role: (Role)999);

        Assert.True(result.IsFailure);

        Assert.Equal(EmployeeErrors.InvalidRole.Code, result.TopError.Code);
    }
}