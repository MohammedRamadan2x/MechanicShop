using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Commands.SettleInvoice;
    
public class SettleInvoiceCommandHandler(
    ILogger<SettleInvoiceCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache,
    TimeProvider timeProvider)
    : IRequestHandler<SettleInvoiceCommand, Result<Success>>
{
    private readonly ILogger<SettleInvoiceCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result<Success>> Handle(
        SettleInvoiceCommand command, 
        CancellationToken ct)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == command.InvoiceId, ct);

        if (invoice is null)
        {
            _logger.LogWarning("Invoice {InvoiceId} not found.", command.InvoiceId);

            return ApplicationErrors.InvoiceNotFound;
        }

        var payInvoiceResult = invoice.MarkAsPaid(_timeProvider);

        if (payInvoiceResult.IsFailure)
        {
            _logger.LogWarning(
                "Invoice payment failed for InvoiceId: {InvoiceId}. " +
                "Errors: {@Errors}",
                invoice.Id,
                payInvoiceResult.Errors);

            return payInvoiceResult.Errors;
        }

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("invoice", ct);

        _logger.LogInformation("Invoice {InvoiceId} successfully paid.", invoice.Id);

        return Result.Success;
    }
}