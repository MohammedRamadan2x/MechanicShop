using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing.Enums;

namespace MechanicShop.Domain.WorkOrders.Billing;

public sealed class Invoice : AuditableEntity
{
    public Guid WorkOrderId { get; }
    public DateTimeOffset IssuedAtUtc { get; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; }
    public decimal Subtotal => LineItems.Sum(x => x.LineTotal);
    public decimal Total => Subtotal - DiscountAmount + TaxAmount;

    public DateTimeOffset? PaidAt { get; private set; }

    public WorkOrder? WorkOrder { get; set; }

    private readonly List<InvoiceLineItem> _lineItems = [];
    public IReadOnlyList<InvoiceLineItem> LineItems => _lineItems;

    public InvoiceStatus Status { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private Invoice()
    { }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private Invoice(
        Guid id,
        Guid workOrderId,
        DateTimeOffset issuedAt,
        List<InvoiceLineItem> lineItems,
        decimal discountAmount,
        decimal taxAmount) : base(id)
    {
        WorkOrderId = workOrderId;
        IssuedAtUtc = issuedAt;
        DiscountAmount = discountAmount;
        Status = InvoiceStatus.Unpaid;
        TaxAmount = taxAmount;
        _lineItems = new List<InvoiceLineItem>(lineItems);
    }

    public static Result<Invoice> Create(
        Guid id,
        Guid workOrderId,
        List<InvoiceLineItem> items,
        decimal discountAmount,
        decimal taxAmount,
        TimeProvider timeProvider)
    {
        if (id == Guid.Empty)
        {
            return InvoiceErrors.IdRequired;
        }

        if (workOrderId == Guid.Empty)
        {
            return InvoiceErrors.WorkOrderIdRequired;
        }

        if (items is null || items.Count == 0)
        {
            return InvoiceErrors.LineItemsEmpty;
        }

        if (items.Any(i => i is null))
        {
            return InvoiceErrors.InvalidLineItem;
        }

        if (discountAmount < 0)
        {
            return InvoiceErrors.InvalidDiscount;
        }

        decimal subtotal = items.Sum(i => i.LineTotal);

        if (discountAmount > subtotal)
        {
            return InvoiceErrors.DiscountExceedsSubtotal;
        }

        if (taxAmount < 0)
        {
            return InvoiceErrors.InvalidTaxAmount;
        }

        return new Invoice(
            id, 
            workOrderId, 
            timeProvider.GetUtcNow(), 
            items, 
            discountAmount, 
            taxAmount);
    }

    public Result<Updated> ApplyDiscount(decimal discountAmount)
    {
        if (Status != InvoiceStatus.Unpaid)
        {
            return InvoiceErrors.InvoiceLocked;
        }

        if (discountAmount < 0)
        {
            return InvoiceErrors.InvalidDiscount;
        }

        if (discountAmount > Subtotal)
        {
            return InvoiceErrors.DiscountExceedsSubtotal;
        }

        DiscountAmount = discountAmount;

        return Result.Updated;
    }

    public Result<Updated> MarkAsPaid(TimeProvider timeProvider)
    {
        if (Status != InvoiceStatus.Unpaid)
        {
            return InvoiceErrors.InvoiceLocked;
        }

        Status = InvoiceStatus.Paid;
        PaidAt = timeProvider.GetUtcNow();

        return Result.Updated;
    }
}