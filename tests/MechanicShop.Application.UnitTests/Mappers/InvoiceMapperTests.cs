using MechanicShop.Application.Features.Billing.Mappers;

using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.Billing;

using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;
public class InvoiceMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var labor = EmployeeFactory.CreateLabor().Value;
        var vehicle = customer.Vehicles.First();

        vehicle.Customer = customer;

        var repairTask = RepairTaskFactory.CreateRepairTask(
            laborCost: 150m).Value;

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            laborId: labor.Id,
            repairTasks: [repairTask]).Value;

        workOrder.Vehicle = vehicle;
        workOrder.Labor = labor;

        var invoiceLine = InvoiceLineItemFactory.CreateInvoiceLineItem(
            lineNumber: 1,
            description: "Oil Change",
            quantity: 2,
            unitPrice: 100m).Value;

        var invoice = InvoiceFactory.CreateInvoice(
            workOrderId: workOrder.Id,
            items: [invoiceLine]).Value;

        invoice.WorkOrder = workOrder;

        var dto = invoice.ToDto();

        Assert.Equal(invoice.Id, dto.InvoiceId);
        Assert.Equal(invoice.WorkOrderId, dto.WorkOrderId);
        Assert.Equal(invoice.IssuedAtUtc, dto.IssuedAtUtc);
        Assert.Equal(invoice.Subtotal, dto.Subtotal);
        Assert.Equal(invoice.TaxAmount, dto.TaxAmount);
        Assert.Equal(invoice.DiscountAmount, dto.DiscountAmount);
        Assert.Equal(invoice.Total, dto.Total);
        Assert.Equal(invoice.Status.ToString(), dto.PaymentStatus);

        Assert.NotNull(dto.Customer);
        Assert.Equal(customer.Id, dto.Customer!.CustomerId);
        Assert.Equal(customer.Name, dto.Customer.Name);
        Assert.Equal(customer.Email, dto.Customer.Email);
        Assert.Equal(customer.PhoneNumber, dto.Customer.PhoneNumber);

        Assert.NotNull(dto.Vehicle);
        Assert.Equal(vehicle.Id, dto.Vehicle!.VehicleId);
        Assert.Equal(vehicle.Make, dto.Vehicle.Make);
        Assert.Equal(vehicle.Model, dto.Vehicle.Model);
        Assert.Equal(vehicle.Year, dto.Vehicle.Year);
        Assert.Equal(vehicle.LicensePlate, dto.Vehicle.LicensePlate);

        Assert.Single(dto.Items);

        var itemDto = dto.Items[0];

        Assert.Equal(invoiceLine.InvoiceId, itemDto.InvoiceId);
        Assert.Equal(invoiceLine.LineNumber, itemDto.LineNumber);
        Assert.Equal(invoiceLine.Description, itemDto.Description);
        Assert.Equal(invoiceLine.Quantity, itemDto.Quantity);
        Assert.Equal(invoiceLine.UnitPrice, itemDto.UnitPrice);
        Assert.Equal(invoiceLine.LineTotal, itemDto.LineTotal);
    }

    [Fact]
    public void ToDtos_ShouldMapListCorrectly()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var labor = EmployeeFactory.CreateLabor().Value;
        var vehicle = customer.Vehicles.First();

        vehicle.Customer = customer;

        var repairTask = RepairTaskFactory.CreateRepairTask(
            laborCost: 100m,
            parts: [PartFactory.CreatePart(cost: 50, quantity: 1).Value]).Value;

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            laborId: labor.Id,
            repairTasks: [repairTask]).Value;

        workOrder.Vehicle = vehicle;
        workOrder.Labor = labor;

        var invoiceLine = InvoiceLineItemFactory.CreateInvoiceLineItem(
            lineNumber: 1,
            description: "Oil Change",
            quantity: 1,
            unitPrice: 150m).Value;

        var invoice = InvoiceFactory.CreateInvoice(
            workOrderId: workOrder.Id,
            items: [invoiceLine]).Value;

        invoice.WorkOrder = workOrder;

        var invoices = new List<Invoice> { invoice };

        var dtos = invoices.ToDtos();

        Assert.Single(dtos);

        var dto = dtos[0];

        Assert.Equal(invoice.Id, dto.InvoiceId);
        Assert.Equal(invoice.WorkOrderId, dto.WorkOrderId);
        Assert.Equal(invoice.IssuedAtUtc, dto.IssuedAtUtc);
        Assert.Equal(invoice.Subtotal, dto.Subtotal);
        Assert.Equal(invoice.TaxAmount, dto.TaxAmount);
        Assert.Equal(invoice.DiscountAmount, dto.DiscountAmount);
        Assert.Equal(invoice.Total, dto.Total);
        Assert.Equal(invoice.Status.ToString(), dto.PaymentStatus);

        Assert.NotNull(dto.Customer);
        Assert.Equal(customer.Id, dto.Customer!.CustomerId);

        Assert.NotNull(dto.Vehicle);
        Assert.Equal(vehicle.Id, dto.Vehicle!.VehicleId);

        Assert.Single(dto.Items);
        Assert.Equal(invoiceLine.LineNumber, dto.Items[0].LineNumber);
        Assert.Equal(invoiceLine.Description, dto.Items[0].Description);
        Assert.Equal(invoiceLine.Quantity, dto.Items[0].Quantity);
        Assert.Equal(invoiceLine.UnitPrice, dto.Items[0].UnitPrice);
        Assert.Equal(invoiceLine.LineTotal, dto.Items[0].LineTotal);
    }

    [Fact]
    public void ToDto_ShouldThrowArgumentNullException_WhenInvoiceIsNull()
    {
        Invoice invoice = null!;

        Assert.Throws<ArgumentNullException>(() => invoice.ToDto());
    }

    [Fact]
    public void ToDto_ShouldMapLineItemCorrectly()
    {
        var invoiceLine = InvoiceLineItemFactory.CreateInvoiceLineItem(
            lineNumber: 1,
            description: "Brake Service",
            quantity: 2,
            unitPrice: 150m).Value;

        var dto = invoiceLine.ToDto();

        Assert.Equal(invoiceLine.InvoiceId, dto.InvoiceId);
        Assert.Equal(invoiceLine.LineNumber, dto.LineNumber);
        Assert.Equal(invoiceLine.Description, dto.Description);
        Assert.Equal(invoiceLine.Quantity, dto.Quantity);
        Assert.Equal(invoiceLine.UnitPrice, dto.UnitPrice);
        Assert.Equal(invoiceLine.LineTotal, dto.LineTotal);
    }

    [Fact]
    public void ToDtos_ShouldMapLineItemsListCorrectly()
    {
        var item1 = InvoiceLineItemFactory.CreateInvoiceLineItem(
            lineNumber: 1,
            description: "Oil Change",
            quantity: 1,
            unitPrice: 100m).Value;

        var item2 = InvoiceLineItemFactory.CreateInvoiceLineItem(
            lineNumber: 2,
            description: "Brake Service",
            quantity: 2,
            unitPrice: 200m).Value;

        var items = new List<InvoiceLineItem>
        {
            item1,
            item2
        };

        var dtos = items.ToDtos();

        Assert.Equal(items.Count, dtos.Count);

        Assert.Equal(item1.InvoiceId, dtos[0].InvoiceId);
        Assert.Equal(item1.LineNumber, dtos[0].LineNumber);
        Assert.Equal(item1.Description, dtos[0].Description);
        Assert.Equal(item1.Quantity, dtos[0].Quantity);
        Assert.Equal(item1.UnitPrice, dtos[0].UnitPrice);
        Assert.Equal(item1.LineTotal, dtos[0].LineTotal);

        Assert.Equal(item2.InvoiceId, dtos[1].InvoiceId);
        Assert.Equal(item2.LineNumber, dtos[1].LineNumber);
        Assert.Equal(item2.Description, dtos[1].Description);
        Assert.Equal(item2.Quantity, dtos[1].Quantity);
        Assert.Equal(item2.UnitPrice, dtos[1].UnitPrice);
        Assert.Equal(item2.LineTotal, dtos[1].LineTotal);
    }

    [Fact]
    public void ToDto_ShouldThrowArgumentNullException_WhenLineItemIsNull()
    {
        InvoiceLineItem invoiceLineItem = null!;

        Assert.Throws<ArgumentNullException>(() => invoiceLineItem.ToDto());
    }
}