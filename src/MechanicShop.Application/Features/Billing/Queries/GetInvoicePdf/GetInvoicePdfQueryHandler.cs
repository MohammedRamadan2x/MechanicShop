using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;

public class GetInvoicePdfQueryHandler(
    ILogger<GetInvoicePdfQueryHandler> logger,
    IInvoicePdfGenerator pdfGenerator,
    IAppDbContext context)
    :IRequestHandler<GetInvoicePdfQuery, Result<InvoicePdfDto>>
{
    private readonly ILogger<GetInvoicePdfQueryHandler> _logger = logger;
    private readonly IInvoicePdfGenerator _pdfGenerator = pdfGenerator;
    private readonly IAppDbContext _context = context;

    public async Task<Result<InvoicePdfDto>> Handle(
        GetInvoicePdfQuery query, 
        CancellationToken ct)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, ct);

        if (invoice is null)
        {
            _logger.LogWarning(
                "Invoice not found. " +
                "InvoiceId: {InvoiceId}", 
                query.InvoiceId);

            return ApplicationErrors.InvoiceNotFound;
        }

        try
        {
            var pdfBytes = _pdfGenerator.Generate(invoice);

            var invoicePdf = new InvoicePdfDto
            {
                Content = pdfBytes,
                FileName = $"invoice-{invoice.Id}.pdf"
            };

            return invoicePdf;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Failed to generate PDF for InvoiceId: {InvoiceId}", 
                query.InvoiceId);

            return Error.Failure("An error occurred while generating the invoice PDF.");
        }
    }
}
