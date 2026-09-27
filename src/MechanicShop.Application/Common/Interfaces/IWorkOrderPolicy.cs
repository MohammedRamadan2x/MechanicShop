using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Common.Interfaces;

public interface IWorkOrderPolicy
{
    bool IsOutsideOperatingHours(
        DateTimeOffset startAt, 
        TimeSpan duration);

    Task<bool> IsLaborOccupied(
        Guid laborId, 
        Guid excludedWorkOrderId, 
        DateTimeOffset startAt, 
        DateTimeOffset endAt,
        CancellationToken ct = default);

    Task<bool> IsVehicleAlreadyScheduled(
        Guid vehicleId, 
        DateTimeOffset startAt, 
        DateTimeOffset endAt, 
        Guid? excludedWorkOrderId = null,
        CancellationToken ct = default);

    Task<Result<Success>> CheckSpotAvailabilityAsync(
        Spot spot, 
        DateTimeOffset startAt, 
        DateTimeOffset endAt, 
        Guid? excludeWorkOrderId = null,
        CancellationToken ct = default);

    Result<Success> ValidateMinimumRequirement(
        DateTimeOffset startAt, 
        DateTimeOffset endAt);
}