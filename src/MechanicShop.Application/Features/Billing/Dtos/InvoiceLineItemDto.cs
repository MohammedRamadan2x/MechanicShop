namespace MechanicShop.Application.Features.Billing.Dtos;

public sealed record InvoiceLineItemDto
{
    public Guid InvoiceId { get; init; }
    public int LineNumber { get; init; }
    public string? Description { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}