using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;

namespace MechanicShop.Tests.Common.Employees;

public static class EmployeeFactory
{
    public static Result<Employee> CreateEmployee(
        Guid? id = null, 
        string? firstName = null, 
        string? lastName = null, 
        Role? role = null)
    {
        return Employee.Create(
            id ?? Guid.NewGuid(),
            firstName ?? "John",
            lastName ?? "Doe",
            role ?? Role.Labor);
    }

    public static Result<Employee> CreateLabor(
        Guid? id = null, 
        string? firstName = null, 
        string? lastName = null)
    {
        return CreateEmployee(
            id ?? Guid.NewGuid(),
            firstName ?? "John",
            lastName ?? "Labor",
            Role.Labor);
    }

    public static Result<Employee> CreateManager(
        Guid? id = null, 
        string? firstName = null, 
        string? lastName = null)
    {
        return CreateEmployee(
            id ?? Guid.NewGuid(),
            firstName ?? "John",
            lastName ?? "Manager",
            Role.Manager);
    }
}