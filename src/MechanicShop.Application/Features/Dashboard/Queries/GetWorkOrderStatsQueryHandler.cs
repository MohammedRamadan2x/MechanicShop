using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Dashboard.Queries;


public class GetWorkOrderStatsQueryHandler(
    ILogger<GetWorkOrderStatsQueryHandler> logger,
    IAppDbContext context) 
    : IRequestHandler<GetWorkOrderStatsQuery, Result<TodayWorkOrderStatsDto>>
{
    private readonly ILogger<GetWorkOrderStatsQueryHandler> _logger = logger;
    private readonly IAppDbContext _context = context;

    public async Task<Result<TodayWorkOrderStatsDto>> Handle(
        GetWorkOrderStatsQuery request, 
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Generating work order statistics for {Date}.",
            request.Date);

        var start = request.Date
            .ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        
        var end = request.Date
            .AddDays(1)
            .ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var query = _context.WorkOrders
            .AsNoTracking()
            .Include(w => w.Vehicle)
            .Include(w => w.RepairTasks)
                .ThenInclude(rt => rt.Parts)
            .Include(w => w.Invoice)
            .Where(w => w.StartAtUtc >= start && w.StartAtUtc < end);

        var total = await query.CountAsync(ct);

        if (total == 0)
        {
            _logger.LogInformation(
                "No work orders found for {Date}.",
                request.Date);

            return new TodayWorkOrderStatsDto
            {
                Date = request.Date,
                Total = 0,
                Scheduled = 0,
                InProgress = 0,
                Completed = 0,
                Cancelled = 0,
                TotalRevenue = 0,
                TotalPartsCost = 0,
                TotalLaborCost = 0,
                UniqueVehicles = 0,
                UniqueCustomers = 0
            };
        }

        var stats = await query.ToListAsync(ct);

        var totalRevenue = stats
            .Sum(x => x.Invoice?.Total ?? 0m);
        
        var totalPartCost = stats
            .Where(x => x.Invoice != null)
            .Sum(x => x.TotalPartsCost ?? 0m);
        
        var totalLaborCost = stats
            .Where(x => x.Invoice != null)
            .Sum(x => x.TotalLaborCost ?? 0m);
        
        var uniqueVehicles = stats
            .Select(x => x.VehicleId)
            .Distinct()
            .Count();
        
        var uniqueCustomers = stats
            .Select(x => x.Vehicle!.CustomerId)
            .Distinct()
            .Count();

        var netProfit = totalRevenue - totalPartCost - totalLaborCost;

        var result = new TodayWorkOrderStatsDto
        {
            Date = request.Date,
            Total = total,
            Scheduled = stats.Count(x => x.State == WorkOrderState.Scheduled),
            InProgress = stats.Count(x => x.State == WorkOrderState.InProgress),
            Completed = stats.Count(x => x.State == WorkOrderState.Completed),
            Cancelled = stats.Count(x => x.State == WorkOrderState.Cancelled),
            TotalRevenue = totalRevenue,
            TotalPartsCost = totalPartCost,
            TotalLaborCost = totalLaborCost,
            UniqueVehicles = uniqueVehicles,
            UniqueCustomers = uniqueCustomers,
            NetProfit = netProfit,
            ProfitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0,
            CompletionRate = total > 0 ? ((decimal)stats.Count(x => x.State == WorkOrderState.Completed) / total) * 100 : 0,
            AverageRevenuePerOrder = total > 0 ? totalRevenue / total : 0,
            OrdersPerVehicle = uniqueVehicles > 0 ? (decimal)total / uniqueVehicles : 0,
            PartsCostRatio = totalRevenue > 0 ? (totalPartCost / totalRevenue) * 100 : 0,
            LaborCostRatio = totalRevenue > 0 ? (totalLaborCost / totalRevenue) * 100 : 0,
            CancellationRate = total > 0 ? ((decimal)stats.Count(x => x.State == WorkOrderState.Cancelled) / total) * 100 : 0
        };

        _logger.LogInformation(
            "Work order statistics generated successfully for {Date}. " +
            "Orders: {Total}, Revenue: {Revenue}, Profit: {Profit}.",
            request.Date,
            total,
            totalRevenue,
            netProfit);

        return result;
    }
}