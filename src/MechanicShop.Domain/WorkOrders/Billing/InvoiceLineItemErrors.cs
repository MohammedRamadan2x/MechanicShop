using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.WorkOrders.Billing;

public static class InvoiceLineItemErrors
{
    public static Error InvoiceIdRequired => 
        Error.Validation("InvoiceLineItem.InvoiceIdRequired", 
            "Invoice ID is required.");

    public static Error InvalidLineNumber => 
        Error.Validation("InvoiceLineItem.InvalidLineNumber", 
            "Line number must be greater than 0.");

    public static Error InvalidQuantity => 
        Error.Validation("InvoiceLineItem.InvalidQuantity", 
            "Quantity must be greater than 0.");

    public static Error InvalidUnitPrice => 
        Error.Validation("InvoiceLineItem.InvalidUnitPrice",
            "Unit price must be greater than 0.");

    public static Error DescriptionRequired => 
        Error.Validation("InvoiceLineItem.DescriptionRequired", 
            "Description is required.");
}