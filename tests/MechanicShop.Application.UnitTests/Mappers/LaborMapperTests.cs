using MechanicShop.Application.Features.Labors.Mappers;

using MechanicShop.Tests.Common.Employees;
using MechanicShop.Domain.Employees;

using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class LaborMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var labor = EmployeeFactory.CreateLabor(
            firstName: "John",
            lastName: "Doe").Value;

        var dto = labor.ToDto();

        Assert.Equal(labor.Id, dto.LaborId);
        Assert.Equal(labor.FullName, dto.Name);
    }

    [Fact]
    public void ToDtos_ShouldMapListCorrectly()
    {
        var labor1 = EmployeeFactory.CreateLabor(
            firstName: "John",
            lastName: "Doe").Value;

        var labor2 = EmployeeFactory.CreateLabor(
            firstName: "Jane",
            lastName: "Smith").Value;

        var labors = new List<Employee>
        {
            labor1,
            labor2
        };

        var dtos = labors.ToDtos();

        Assert.Equal(labors.Count, dtos.Count);

        Assert.Equal(labor1.Id, dtos[0].LaborId);
        Assert.Equal(labor1.FullName, dtos[0].Name);

        Assert.Equal(labor2.Id, dtos[1].LaborId);
        Assert.Equal(labor2.FullName, dtos[1].Name);
    }

    [Fact]
    public void ToDto_ShouldThrowArgumentNullException_WhenEmployeeIsNull()
    {
        Employee employee = null!;

        Assert.Throws<ArgumentNullException>(() => employee.ToDto());
    }
}