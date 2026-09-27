using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.WorkOrders.Billing;

public sealed class InvoiceLineItem
{
    public Guid InvoiceId { get; }
    public int LineNumber { get; }
    public string Description { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal LineTotal => Quantity * UnitPrice;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private InvoiceLineItem()
    { }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private InvoiceLineItem(
        Guid invoiceId, 
        int lineNumber, 
        string description, 
        int quantity, 
        decimal unitPrice)
    {
        InvoiceId = invoiceId;
        LineNumber = lineNumber;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static Result<InvoiceLineItem> Create(
        Guid invoiceId,
        int lineNumber,
        string description,
        int quantity,
        decimal unitPrice)
    {
        if (invoiceId == Guid.Empty)
        {
            return InvoiceLineItemErrors.InvoiceIdRequired;
        }

        if (lineNumber <= 0)
        {
            return InvoiceLineItemErrors.InvalidLineNumber;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return InvoiceLineItemErrors.DescriptionRequired;
        }

        if (quantity <= 0)
        {
            return InvoiceLineItemErrors.InvalidQuantity;
        }

        if (unitPrice <= 0)
        {
            return InvoiceLineItemErrors.InvalidUnitPrice;
        }

        return new InvoiceLineItem(
            invoiceId, 
            lineNumber, 
            description.Trim(), 
            quantity, 
            unitPrice);
    }
}