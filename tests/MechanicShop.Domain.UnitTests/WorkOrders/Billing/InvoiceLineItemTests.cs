using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Tests.Common.Billing;

using Xunit;

namespace MechanicShop.Domain.UnitTests.WorkOrders.Billing;

public class InvoiceLineItemTests
{
    [Fact]
    public void CreateInvoiceLineItem_ShouldSucceed_WithValidData()
    {
        var invoiceId = Guid.NewGuid();
        const int lineNumber = 1;
        const string description = "Brake Pad";
        const int quantity = 2;
        const decimal unitPrice = 50m;

        var result = InvoiceLineItem.Create(
            invoiceId, 
            lineNumber, 
            description, 
            quantity, 
            unitPrice);

        Assert.True(result.IsSuccess);

        var item = result.Value;

        Assert.Equal(invoiceId, item.InvoiceId);
        Assert.Equal(lineNumber, item.LineNumber);
        Assert.Equal(description, item.Description);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(unitPrice, item.UnitPrice);
        Assert.Equal(quantity * unitPrice, item.LineTotal);
    }

    [Fact]
    public void CreateInvoiceLineItem_ShouldFail_WhenEmptyInvoiceId()
    {
        var result = InvoiceLineItemFactory.CreateInvoiceLineItem(id: Guid.Empty);

        Assert.True(result.IsFailure);

        Assert.Equal(InvoiceLineItemErrors.InvoiceIdRequired.Code, 
            result.TopError.Code);
    }

    [Fact]
    public void CreateInvoiceLineItem_ShouldFail_WhenInvalidLineNumber()
    {
        var result = InvoiceLineItemFactory.CreateInvoiceLineItem(lineNumber: 0);

        Assert.True(result.IsFailure);

        Assert.Equal(InvoiceLineItemErrors.InvalidLineNumber.Code, 
            result.TopError.Code);
    }

    [Fact]
    public void CreateInvoiceLineItem_ShouldFail_WhenEmptyDescription()
    {
        var result = InvoiceLineItemFactory.CreateInvoiceLineItem(description: " ");

        Assert.True(result.IsFailure);

        Assert.Equal(InvoiceLineItemErrors.DescriptionRequired.Code, 
            result.TopError.Code);
    }

    [Fact]
    public void CreateInvoiceLineItem_ShouldFail_WhenInvalidQuantity()
    {
        var result = InvoiceLineItemFactory.CreateInvoiceLineItem(quantity: 0);

        Assert.True(result.IsFailure);

        Assert.Equal(InvoiceLineItemErrors.InvalidQuantity.Code, 
            result.TopError.Code);
    }

    [Fact]
    public void CreateInvoiceLineItem_ShouldFail_WhenInvalidUnitPrice()
    {
        var result = InvoiceLineItemFactory.CreateInvoiceLineItem(unitPrice: 0m);

        Assert.True(result.IsFailure);

        Assert.Equal(InvoiceLineItemErrors.InvalidUnitPrice.Code, 
            result.TopError.Code);
    }
}