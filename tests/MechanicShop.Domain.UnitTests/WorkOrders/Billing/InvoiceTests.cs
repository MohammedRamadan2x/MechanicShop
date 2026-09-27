using MechanicShop.Domain.WorkOrders.Billing.Enums;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Tests.Common.Billing;
using MechanicShop.Tests.Common;

using Xunit;

namespace MechanicShop.Domain.UnitTests.WorkOrders.Billing;

public class InvoiceTests()
{
    [Fact]
    public void CreateInvoice_ShouldSucceed_WithValidData()
    {
        var id = Guid.NewGuid();
        var workOrderId = Guid.NewGuid();

        var items = new List<InvoiceLineItem>
        {
            InvoiceLineItem.Create(Guid.NewGuid(), 1, "Oil Change", 2, 50).Value
        };

        var time = new FakeTimeProvider();
        time.SetUtcNow(DateTimeOffset.Parse("2026-01-01T00:00:00Z"));

        var result = InvoiceFactory.CreateInvoice(
            id, 
            workOrderId, 
            items, 
            10, 
            5, 
            time);

        Assert.True(result.IsSuccess);

        var invoice = result.Value;

        Assert.Equal(id, invoice.Id);
        Assert.Equal(workOrderId, invoice.WorkOrderId);
        Assert.Equal(InvoiceStatus.Unpaid, invoice.Status);
        Assert.Equal(10, invoice.DiscountAmount);
        Assert.Equal(5, invoice.TaxAmount);
        Assert.Equal(100, invoice.Subtotal);
        Assert.Equal(95, invoice.Total);
        Assert.Equal(time.GetUtcNow(), invoice.IssuedAtUtc);
    }

    [Fact]
    public void CreateInvoice_ShouldFail_WhenEmptyItems()
    {
        List<InvoiceLineItem> items = [];

        var result = InvoiceFactory.CreateInvoice(items: items);

        Assert.True(result.IsFailure);

        Assert.Equal(InvoiceErrors.LineItemsEmpty.Code, result.TopError.Code);
    }

    [Fact]
    public void ApplyDiscount_ShouldUpdateDiscount_WhenUnpaid()
    {
        const int discount = 15;

        var invoice = InvoiceFactory.CreateInvoice().Value;
        var originalTotal = invoice.Total;

        var result = invoice.ApplyDiscount(discount);

        Assert.True(result.IsSuccess);
        Assert.Equal(discount, invoice.DiscountAmount);
        Assert.Equal(originalTotal - discount, invoice.Total);
    }

    [Fact]
    public void ApplyDiscount_ShouldFail_WhenNegativeAmount()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;

        var result = invoice.ApplyDiscount(-10);

        Assert.True(result.IsFailure);

        Assert.Equal(InvoiceErrors.InvalidDiscount.Code, result.TopError.Code);
    }

    [Fact]
    public void ApplyDiscount_ShouldFail_WhenGreaterThanSubtotal()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;

        var excessiveDiscount = invoice.Subtotal + 1;

        var result = invoice.ApplyDiscount(excessiveDiscount);

        Assert.True(result.IsFailure);

        Assert.Equal(InvoiceErrors.DiscountExceedsSubtotal.Code, result.TopError.Code);
    }

    [Fact]
    public void ApplyDiscount_ShouldSucceed_WithValidAmount()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;

        const decimal validDiscount = 20m;

        var result = invoice.ApplyDiscount(validDiscount);

        Assert.True(result.IsSuccess);

        Assert.Equal(validDiscount, invoice.DiscountAmount);
    }

    [Fact]
    public void ApplyDiscount_ShouldFail_WhenPaid()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;

        Assert.True(invoice.MarkAsPaid(TimeProvider.System).IsSuccess);

        var result = invoice.ApplyDiscount(10);

        Assert.True(result.IsFailure);

        Assert.Equal(InvoiceErrors.InvoiceLocked.Code, result.TopError.Code);
    }

    [Fact]
    public void MarkAsPaid_ShouldSucceed_WhenUnpaid()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;

        var time = new FakeTimeProvider();
        time.SetUtcNow(DateTimeOffset.Parse("2024-01-01T00:00:00Z"));

        var result = invoice.MarkAsPaid(time);

        Assert.True(result.IsSuccess);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(time.GetUtcNow(), invoice.PaidAt);
    }

    [Fact]
    public void MarkAsPaid_ShouldFail_WhenAlreadyPaid()
    {
        var invoice = InvoiceFactory.CreateInvoice().Value;

        Assert.True(invoice.MarkAsPaid(TimeProvider.System).IsSuccess);

        var result = invoice.MarkAsPaid(TimeProvider.System);

        Assert.True(result.IsFailure);

        Assert.Equal(InvoiceErrors.InvoiceLocked.Code, result.TopError.Code);
    }
}