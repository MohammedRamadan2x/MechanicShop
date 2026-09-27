using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Customers.Vehicles;

public sealed class Vehicle : AuditableEntity
{
    public Guid CustomerId { get; }
    public string Make { get; private set; }
    public string Model { get; private set; }
    public int Year { get; private set; }
    public string LicensePlate { get; private set; }
    public Customer? Customer { get; set; }

    public string VehicleInfo => $"{Make} | {Model} | {Year}";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private Vehicle()
    { }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private Vehicle(Guid id, string make, string model, int year, string licensePlate)
        : base(id)
    {
        Make = make;
        Model = model;
        Year = year;
        LicensePlate = licensePlate;
    }

    public static Result<Vehicle> Create(
        Guid id, 
        string make, 
        string model, 
        int year, 
        string licensePlate)
    {
        if (id == Guid.Empty)
        {
            return VehicleErrors.IdRequired;
        }

        if (string.IsNullOrWhiteSpace(make))
        {
            return VehicleErrors.MakeRequired;
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            return VehicleErrors.ModelRequired;
        }

        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return VehicleErrors.LicensePlateRequired;
        }

        if (year < 1886 || year > DateTime.UtcNow.Year)
        {
            return VehicleErrors.InvalidYear;
        }

        return new Vehicle(id, make, model, year, licensePlate);
    }

    public Result<Updated> Update(
        string make, 
        string model, 
        int year, 
        string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(make))
        {
            return VehicleErrors.MakeRequired;
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            return VehicleErrors.ModelRequired;
        }

        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return VehicleErrors.LicensePlateRequired;
        }

        if (year < 1886 || year > DateTime.UtcNow.Year)
        {
            return VehicleErrors.InvalidYear;
        }

        Make = make;
        Model = model;
        Year = year;
        LicensePlate = licensePlate;

        return Result.Updated;
    }
}