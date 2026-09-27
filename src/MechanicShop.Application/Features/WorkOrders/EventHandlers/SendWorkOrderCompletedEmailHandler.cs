using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.EventHandlers;


public sealed class SendWorkOrderCompletedEmailHandler(
    ILogger<SendWorkOrderCompletedEmailHandler> logger,
    INotificationService notificationService,
    IAppDbContext context)
    : INotificationHandler<WorkOrderCompleted>
{
    private readonly ILogger<SendWorkOrderCompletedEmailHandler> _logger = logger;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IAppDbContext _context = context;

    public async Task Handle(
        WorkOrderCompleted notification, 
        CancellationToken ct)
    {
        var workOrder = await _context.WorkOrders
                        .AsNoTracking()
                        .Include(w => w.Vehicle!)
                            .ThenInclude(v => v.Customer)
                        .FirstOrDefaultAsync(w => 
                            w.Id == notification.WorkOrderId, ct);

        if (workOrder is null)
        {
            _logger.LogWarning(
                "WorkOrder with Id '{WorkOrderId}' does not exist.", 
                notification.WorkOrderId);

            return;
        }

        if (!string.IsNullOrWhiteSpace(workOrder.Vehicle?.Customer?.Email))
        {
            await _notificationService
                .SendEmailAsync(workOrder.Vehicle.Customer.Email, ct);
        }

        if (!string.IsNullOrWhiteSpace(workOrder.Vehicle?.Customer?.PhoneNumber))
        {
            await _notificationService
                .SendSmsAsync(workOrder.Vehicle.Customer.PhoneNumber, ct);
        }

        _logger.LogInformation(
            "Notification sent successfully for completed work order {WorkOrderId}.",
            notification.WorkOrderId);
    }
}