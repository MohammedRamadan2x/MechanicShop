using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Infrastructure.Settings;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MechanicShop.Infrastructure.BackgroundJobs;

public class OverdueBookingCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<OverdueBookingCleanupService> logger,
    IOptions<AppSettings> options,
    TimeProvider timeProvider) 
    : BackgroundService
{
    private readonly ILogger<OverdueBookingCleanupService> _logger = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly AppSettings _appSettings = options.Value;
    private readonly TimeProvider _timeProvider = timeProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan
            .FromMinutes(_appSettings.OverdueBookingCleanupFrequencyMinutes));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            _logger.LogInformation(
                "Checking overdue work orders at {Now}", 
                _timeProvider.GetUtcNow());

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

                var cutoff = _timeProvider.GetUtcNow()
                    .AddMinutes(-_appSettings.BookingCancellationThresholdMinutes);

                var overdue = await db.WorkOrders
                    .Where(wo =>
                           wo.State == WorkOrderState.Scheduled &&
                           wo.StartAtUtc <= cutoff)
                    .ToListAsync(stoppingToken);

                if (overdue.Count > 0)
                {
                    foreach (var workOrder in overdue)
                    {
                        var result = workOrder.Cancel();

                        if (result.IsFailure)
                        {
                            _logger.LogWarning(
                                "Failed to cancel WorkOrder {Id}: {Error}", 
                                workOrder.Id, 
                                result.Errors);
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation(
                        "Cancelled {Count} overdue work orders: {Ids}", 
                        overdue.Count, 
                        overdue.Select(wo => wo.Id));
                }
                else
                {
                    _logger.LogInformation("No overdue work orders found.");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up overdue work orders.");

            }
        }
    }
}