namespace MechanicShop.Contracts.Responses;

public sealed record OperatingHoursResponse(
    TimeOnly OpeningTime, 
    TimeOnly ClosingTime);