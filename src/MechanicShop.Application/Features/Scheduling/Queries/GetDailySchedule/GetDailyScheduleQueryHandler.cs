using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Application.Features.Labors.Mappers;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.Scheduling.Queries.GetDailySchedule;

public class GetDailyScheduleQueryHandler(
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<GetDailyScheduleQuery, Result<ScheduleDto>>
{
    private readonly IAppDbContext _context = context;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result<ScheduleDto>> Handle(
        GetDailyScheduleQuery query, 
        CancellationToken ct)
    {
        var localStart = query.ScheduleDate.ToDateTime(TimeOnly.MinValue);
        var localEnd = localStart.AddDays(1);

        var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, query.TimeZone);
        var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, query.TimeZone);

        var workOrders = await _context.WorkOrders
            .AsNoTracking()
            .Where(wo =>           
                wo.StartAtUtc < utcEnd &&
                wo.EndAtUtc > utcStart &&
                (query.LaborId == null || wo.LaborId == query.LaborId))
            .Include(wo => wo.RepairTasks)
            .Include(wo => wo.Vehicle)
            .Include(wo => wo.Labor)
            .ToListAsync(ct);

        var now = TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), query.TimeZone);

        var result = new ScheduleDto
        {
            OnDate = query.ScheduleDate,
            EndOfDay = localEnd < now,
            Spots = []
        };

        foreach (var spot in Enum.GetValues<Spot>())
        {
            var current = localStart;
            var slots = new List<AvailabilitySlotDto>();

            var workOrdersBySpot = workOrders
                .Where(wo => wo.Spot == spot)
                .OrderBy(wo => wo.StartAtUtc)
                .ToList();

            while (current < localEnd)
            {
                var next = current.AddMinutes(15);
                var startUtc = TimeZoneInfo.ConvertTimeToUtc(current, query.TimeZone);
                var endUtc = TimeZoneInfo.ConvertTimeToUtc(next, query.TimeZone);

                var workOrder = workOrdersBySpot
                    .FirstOrDefault(wo =>
                        wo.StartAtUtc < endUtc && wo.EndAtUtc > startUtc);

                if (workOrder is not null)
                {
                    if (!slots.Any(s => s.WorkOrderId == workOrder.Id))
                    {
                        slots.Add(new AvailabilitySlotDto
                        {
                            WorkOrderId = workOrder.Id,
                            Spot = spot,
                            StartAt = workOrder.StartAtUtc,
                            EndAt = workOrder.EndAtUtc,
                            Vehicle = FormatVehicleInfo(workOrder.Vehicle!),
                            Labor = workOrder.Labor!.ToDto(),
                            IsOccupied = true,
                            RepairTasks = workOrder.RepairTasks
                                .Select(rt => rt.ToDto())
                                .ToArray(),
                            WorkOrderLocked = !workOrder.IsEditable,
                            State = workOrder.State,
                            IsAvailable = false
                        });
                    }
                }
                else
                {
                    slots.Add(new AvailabilitySlotDto
                    {
                        Spot = spot,
                        StartAt = startUtc,
                        EndAt = endUtc,
                        WorkOrderLocked = false,
                        IsAvailable = current >= now
                    });
                }

                current = next;
            }
                
            result.Spots.Add(new SpotDto
            {
                Spot = spot,
                Slots = slots,
            });
        }

        return result;
    }

    private static string? FormatVehicleInfo(Vehicle vehicle) =>
        vehicle != null ? $"{vehicle.Make} | {vehicle.LicensePlate}" : null;
}
