using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Tests.Common.Billing;

public static class InvoiceFactory
{
    public static Result<Invoice> CreateInvoice(
        Guid? id = null,
        Guid? workOrderId = null,
        List<InvoiceLineItem>? items = null,
        decimal? discount = null,
        decimal? taxAmount = null,
        TimeProvider? timeProvider = null)
    {
        return Invoice.Create(
            id ?? Guid.NewGuid(), 
            workOrderId ?? Guid.NewGuid(), 
            items ?? [InvoiceLineItemFactory.CreateInvoiceLineItem().Value], 
            discount ?? 0, 
            taxAmount ?? 0, 
            timeProvider ?? TimeProvider.System);
    }
}