namespace MechanicShop.Client.Models;

public record InvoiceLineItemModel
{
    public Guid InvoiceId { get; set; }
    public int LineNumber { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}