using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Common.Errors;

public static class ApplicationErrors
{
    public static Error WorkOrderOutsideOperatingHour(DateTimeOffset startAtUtc, DateTimeOffset endAtUtc) =>
        Error.Conflict("ApplicationErrors.WorkOrder.Outside.OperatingHours", 
            $"The WorkOrder time ({startAtUtc} to {endAtUtc}) is outside " +
            $"of store operating hours.");

    public static Error WorkOrderNotFound => 
        Error.NotFound("ApplicationErrors.WorkOrder.NotFound", 
            "WorkOrder does not exist.");

    public static Error LaborOccupied =>
        Error.Conflict("Employee.LaborOccupied", 
            "Labor is already occupied during the requested time.");

    public static Error CustomerNotFound =>
        Error.NotFound("ApplicationErrors.Customer.NotFound", 
            "Customer does not exist.");

    public static Error VehicleNotFound =>
        Error.NotFound("ApplicationErrors.Vehicle.NotFound", "Vehicle does not exist.");

    public static Error VehicleSchedulingConflict =>
        Error.Conflict("Vehicle_Overlapping_WorkOrder", 
            "The vehicle already has an overlapping WorkOrder.");

    public static Error RepairTaskNotFound =>
        Error.NotFound("RepairTask.NotFound", "Repair task does not exist.");

    public static Error PartNotFound =>
        Error.NotFound("Part.NotFound", "Part was not found.");

    public static Error WorkOrderMustBeCompletedForInvoicing =>
        Error.Conflict("WorkOrder.InvoiceIssuance.InvalidState", 
            "WorkOrder must be in 'Completed' state to issue an invoice.");

    public static Error InvoiceNotFound => 
        Error.NotFound("ApplicationErrors.Invoice.NotFound", "Invoice does not exist.");

    public static Error InvalidRefreshToken =>
        Error.Validation("RefreshToken.Expiry.Invalid", "Expiry must be in the future.");

    public static Error ExpiredAccessTokenInvalid =>
        Error.Conflict("Auth.ExpiredAccessToken.Invalid", 
            "Expired access token is not valid.");

    public static Error UserIdClaimInvalid => 
        Error.Conflict("Auth.UserIdClaim.Invalid", "Invalid userId claim.");

    public static Error RefreshTokenExpired => 
        Error.Conflict("Auth.RefreshToken.Expired", 
            "Refresh token is invalid or has expired.");

    public static Error UserNotFound =>
        Error.NotFound("Auth.User.NotFound", "User not found.");

    public static Error TokenGenerationFailed => 
        Error.Failure("Auth.TokenGeneration.Failed", 
            "Failed to generate new JWT token.");

    public static Error LaborNotFound =>
        Error.NotFound("Employee.LaborNotFound", "Labor does not exist.");
}