using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Infrastructure.Settings;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Domain.Common.Results;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MechanicShop.Infrastructure.Services;

public class WorkOrderPolicy(
    IOptions<AppSettings> options,
    IAppDbContext context) 
    : IWorkOrderPolicy
{
    private readonly AppSettings _appSettings = options.Value;
    private readonly IAppDbContext _context = context;

    public async Task<Result<Success>> CheckSpotAvailabilityAsync(
        Spot spot, 
        DateTimeOffset startAt, 
        DateTimeOffset endAt, 
        Guid? excludeWorkOrderId = null, 
        CancellationToken ct = default)
    {
        var isOccupied = await _context.WorkOrders.AnyAsync(
            wo =>
                wo.Spot == spot &&
                wo.StartAtUtc < endAt &&
                wo.EndAtUtc > startAt &&
                (!excludeWorkOrderId.HasValue || wo.Id != excludeWorkOrderId.Value),
            ct);

        return isOccupied
            ? Error.Conflict(
                "MechanicShop.SpotFull", 
                "The selected time slot is unavailable for the requested services.")
            : Result.Success;
    }

    public async Task<bool> IsLaborOccupied(
        Guid laborId, 
        Guid excludedWorkOrderId, 
        DateTimeOffset startAt, 
        DateTimeOffset endAt,
        CancellationToken ct = default)
    {
        return await _context.WorkOrders.AnyAsync(
            wo =>
                wo.LaborId == laborId &&
                wo.Id != excludedWorkOrderId &&
                wo.StartAtUtc < endAt &&
                wo.EndAtUtc > startAt,
            ct);
    }

    public bool IsOutsideOperatingHours(DateTimeOffset startAt, TimeSpan duration)
    {
        var opening = startAt.Date.Add(_appSettings.OpeningTime.ToTimeSpan());
        var closing = startAt.Date.Add(_appSettings.ClosingTime.ToTimeSpan());
        var endAt = startAt + duration;

        return startAt < opening || endAt > closing;
    }

    public async Task<bool> IsVehicleAlreadyScheduled(
        Guid vehicleId, 
        DateTimeOffset startAt, 
        DateTimeOffset endAt, 
        Guid? excludedWorkOrderId = null,
        CancellationToken ct = default)
    {
        return await _context.WorkOrders.AnyAsync(
            wo =>
                wo.VehicleId == vehicleId &&
                wo.StartAtUtc < endAt &&
                wo.EndAtUtc > startAt &&
                (!excludedWorkOrderId.HasValue || wo.Id != excludedWorkOrderId.Value),
            ct);
    }

    public Result<Success> ValidateMinimumRequirement(
        DateTimeOffset startAt, 
        DateTimeOffset endAt)
    {
        if ((endAt - startAt) 
            < 
            TimeSpan.FromMinutes(_appSettings.MinimumAppointmentDurationInMinutes))
        {
            return Error.Conflict(
                "WorkOrder.TooShort",
                $"WorkOrder duration must be at least " +
                $"{_appSettings.MinimumAppointmentDurationInMinutes} minutes.");
        }

        return Result.Success;
    }
}
