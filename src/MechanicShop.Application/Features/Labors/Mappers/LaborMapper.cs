using MechanicShop.Application.Features.Labors.Dtos;
using MechanicShop.Domain.Employees;

namespace MechanicShop.Application.Features.Labors.Mappers;

public static class LaborMapper
{
    public static LaborDto ToDto(this Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        return new LaborDto { LaborId = employee.Id, Name = employee.FullName };
    }

    public static List<LaborDto> ToDtos(this IEnumerable<Employee> employees)
    {
        return [.. employees.Select(l => l.ToDto())];
    }
}