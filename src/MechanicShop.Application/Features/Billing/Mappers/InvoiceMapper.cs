using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Application.Features.Billing.Mappers;

public static class InvoiceMapper
{
    public static InvoiceDto ToDto(this Invoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        return new InvoiceDto
        {
            InvoiceId = invoice.Id,
            WorkOrderId = invoice.WorkOrderId,
            Customer = invoice.WorkOrder!.Vehicle!.Customer!.ToDto(),
            Vehicle = invoice.WorkOrder.Vehicle!.ToDto(),
            IssuedAtUtc = invoice.IssuedAtUtc,
            Subtotal = invoice.Subtotal,
            TaxAmount = invoice.TaxAmount,
            DiscountAmount = invoice.DiscountAmount,
            Total = invoice.Total,
            PaymentStatus = invoice.Status.ToString(),
            Items = invoice.LineItems.Select(x => x.ToDto()).ToList()
        };
    }

    public static List<InvoiceDto> ToDtos(this IEnumerable<Invoice> invoices)
    {
        return [.. invoices.Select(i => i.ToDto())];
    }

    public static InvoiceLineItemDto ToDto(this InvoiceLineItem invoiceLineItem)
    {
        ArgumentNullException.ThrowIfNull(invoiceLineItem);

        return new InvoiceLineItemDto
        {
            InvoiceId = invoiceLineItem.InvoiceId,
            LineNumber = invoiceLineItem.LineNumber,
            Description = invoiceLineItem.Description,
            Quantity = invoiceLineItem.Quantity,
            UnitPrice = invoiceLineItem.UnitPrice,
            LineTotal = invoiceLineItem.LineTotal
        };
    }

    public static List<InvoiceLineItemDto> ToDtos(
        this IEnumerable<InvoiceLineItem> invoiceLineItems)
    {
        return [.. invoiceLineItems.Select(i => i.ToDto())];
    }
}