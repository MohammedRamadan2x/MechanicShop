using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.WorkOrders.Billing;

public static class InvoiceErrors
{
    public static Error IdRequired =>
        Error.Validation("Invoice.IdRequired", "Invoice ID is required.");

    public static Error WorkOrderIdRequired => 
        Error.Validation("Invoice.WorkOrderIdRequired", "Work order ID is required.");

    public static Error LineItemsEmpty => 
        Error.Validation("Invoice.LineItemsEmpty", 
            "Invoice must have at least one line item.");

    public static Error InvalidLineItem =>
        Error.Validation("Invoice.InvalidLineItem", 
            "Invoice contains an invalid line item.");

    public static Error InvalidDiscount => 
        Error.Validation("Invoice.InvalidDiscount", "Discount cannot be negative.");

    public static Error DiscountExceedsSubtotal => 
        Error.Validation("Invoice.DiscountExceedsSubtotal",
            "Discount cannot exceed the subtotal.");

    public static Error InvalidTaxAmount =>
        Error.Validation("Invoice.InvalidTaxAmount", "Tax amount cannot be negative.");

    public static Error InvoiceLocked => 
        Error.Conflict("Invoice.Locked", "Invoice is locked.");
}