using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Customers;

public static class CustomerErrors
{
    public static Error IdRequired =>
        Error.Validation("Customer.IdRequired", "Customer ID is required.");

    public static Error NameRequired =>
        Error.Validation("Customer.NameRequired", "Customer name is required.");

    public static Error PhoneNumberRequired =>
        Error.Validation("Customer.PhoneNumberRequired", "Phone number is required.");

    public static Error EmailRequired =>
        Error.Validation("Customer.EmailRequired", "Email is required.");

    public static Error InvalidEmail =>
      Error.Validation("Customer.InvalidEmail", "Email is invalid.");

    public static Error InvalidPhoneNumber =>
        Error.Validation("Customer.InvalidPhoneNumber", 
            "Phone number must be 7–15 digits and may start with '+'.");

    public static Error VehiclesRequired =>
    Error.Validation("Customer.VehiclesRequired",
        "Vehicles collection is required.");

    public static Error InvalidVehicle =>
        Error.Validation("Customer.InvalidVehicle",
            "Vehicles collection contains an invalid vehicle.");

    public static Error EmailAlreadyExists =>
        Error.Conflict("Customer.EmailAlreadyExists", 
            "A customer with this email already exists.");

    public static Error CannotDeleteCustomerWithWorkOrders =>
        Error.Conflict("Customer.CannotDeleteCustomerWithWorkOrders", 
            "Customer cannot be deleted because they have existing work orders.");
}