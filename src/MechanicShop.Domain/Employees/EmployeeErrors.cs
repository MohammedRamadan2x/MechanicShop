using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Employees;

public static class EmployeeErrors
{
    public static Error IdRequired =>
        Error.Validation("Employee.IdRequired", "Employee ID is required.");

    public static Error FirstNameRequired =>
        Error.Validation("Employee.FirstNameRequired", "First name is required.");

    public static Error LastNameRequired =>
        Error.Validation("Employee.LastNameRequired", "Last name is required.");

    public static Error InvalidRole =>
        Error.Validation("Employee.InvalidRole", "Employee role is invalid.");
}