using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Customers.Vehicles;

public static class VehicleErrors
{
    public static Error IdRequired =>
        Error.Validation("Vehicle.IdRequired", "Vehicle ID is required.");

    public static Error MakeRequired =>
        Error.Validation("Vehicle.MakeRequired", "Vehicle make is required.");

    public static Error ModelRequired =>
        Error.Validation("Vehicle.ModelRequired", "Vehicle model is required.");

    public static Error LicensePlateRequired =>
        Error.Validation("Vehicle.LicensePlateRequired", 
            "Vehicle license plate is required.");

    public static Error InvalidYear =>
        Error.Validation("Vehicle.InvalidYear",
            "Year must be between 1886 and the current year.");
}