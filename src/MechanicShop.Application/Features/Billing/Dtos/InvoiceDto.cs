using MechanicShop.Application.Features.Customers.Dtos;

namespace MechanicShop.Application.Features.Billing.Dtos;

public sealed class InvoiceDto
{
    public Guid InvoiceId { get; init; }
    public Guid WorkOrderId { get; init; }
    public DateTimeOffset IssuedAtUtc { get; init; }
    public CustomerDto? Customer { get; init; }
    public VehicleDto? Vehicle { get; init; }
    public decimal? DiscountAmount { get; init; }
    public decimal Subtotal { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal Total { get; init; }
    public string? PaymentStatus { get; init; }

    public List<InvoiceLineItemDto> Items { get; init; } = [];
}