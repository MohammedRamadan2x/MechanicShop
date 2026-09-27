using MechanicShop.Application.Features.WorkOrders.Mappers;

using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.Billing;
using MechanicShop.Domain.WorkOrders;

using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class WorkOrderMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var labor = EmployeeFactory.CreateLabor().Value;
        var vehicle = customer.Vehicles.First();
        var part = PartFactory.CreatePart(cost: 100, quantity: 2).Value;

        var repairTask = RepairTaskFactory.CreateRepairTask(
            laborCost: 150m,
            parts: [part]).Value;

        var totalPartsCost = part.Cost * part.Quantity;
        var totalLaborCost = repairTask.LaborCost;
        var totalCost = totalPartsCost + totalLaborCost;
        var duration = (int)repairTask.EstimatedDurationInMins;

        var invoiceLine = InvoiceLineItemFactory.CreateInvoiceLineItem(
            lineNumber: 1,
            description: "some description",
            quantity: 1,
            unitPrice: totalCost).Value;

        var invoice = InvoiceFactory.CreateInvoice(
            workOrderId: Guid.NewGuid(),
            items: [invoiceLine]).Value;

        var workOrder = WorkOrderFactory.CreateWorkOrder(
            vehicleId: vehicle.Id,
            laborId: labor.Id,
            repairTasks: [repairTask]).Value;

        workOrder.Vehicle = vehicle;
        workOrder.Labor = labor;
        workOrder.Invoice = invoice;

        var dto = workOrder.ToDto();

        Assert.Equal(workOrder.Id, dto.WorkOrderId);
        Assert.Equal(workOrder.Spot, dto.Spot);
        Assert.Equal(workOrder.StartAtUtc, dto.StartAtUtc);
        Assert.Equal(workOrder.EndAtUtc, dto.EndAtUtc);
        Assert.Equal(workOrder.State, dto.State);
        Assert.Equal(workOrder.CreatedAtUtc, dto.CreatedAt);

        Assert.NotNull(dto.Labor);
        Assert.Equal(workOrder.LaborId, dto.Labor!.LaborId);
        Assert.Equal($"{labor.FirstName} {labor.LastName}", dto.Labor.Name);

        Assert.NotNull(dto.Vehicle);
        Assert.Equal(vehicle.Id, dto.Vehicle!.VehicleId);
        Assert.Equal(vehicle.Make, dto.Vehicle.Make);
        Assert.Equal(vehicle.Model, dto.Vehicle.Model);
        Assert.Equal(vehicle.Year, dto.Vehicle.Year);
        Assert.Equal(vehicle.LicensePlate, dto.Vehicle.LicensePlate);

        Assert.Single(dto.RepairTasks);
        Assert.Equal(totalPartsCost, dto.TotalPartCost);
        Assert.Equal(totalLaborCost, dto.TotalLaborCost);
        Assert.Equal(totalCost, dto.TotalCost);
        Assert.Equal(duration, dto.TotalDurationInMins);
        Assert.Equal(invoice.Id, dto.InvoiceId);
    }

    [Fact]
    public void ToDtos_ShouldMapListCorrectly()
    {
        // Arrange
        var customer = CustomerFactory.CreateCustomer().Value;
        var labor = EmployeeFactory.CreateLabor().Value;
        var vehicle = customer.Vehicles.First();

        var repairTask = RepairTaskFactory.CreateRepairTask(
            laborCost: 100m,
            parts: [PartFactory.CreatePart(cost: 50, quantity: 1).Value]).Value;

        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: vehicle.Id, laborId: labor.Id, repairTasks: [repairTask]).Value;

        workOrder.Vehicle = vehicle;
        workOrder.Labor = labor;

        var workOrders = new List<WorkOrder> { workOrder };

        // Act
        var dtos = workOrders.ToDtos();

        // Assert
        Assert.Single(dtos);
        var dto = dtos[0];

        Assert.Equal(workOrder.Id, dto.WorkOrderId);
        Assert.Equal(workOrder.Spot, dto.Spot);
        Assert.Equal(workOrder.StartAtUtc, dto.StartAtUtc);
        Assert.Equal(workOrder.EndAtUtc, dto.EndAtUtc);
        Assert.NotNull(dto.Labor);
        Assert.Equal($"{labor.FirstName} {labor.LastName}", dto.Labor!.Name);
        Assert.Equal(labor.Id, dto.Labor.LaborId);
        Assert.NotNull(dto.Vehicle);
        Assert.Single(dto.RepairTasks);
        Assert.Equal(workOrder.State, dto.State);
    }

    [Fact]
    public void ToListItemDto_ShouldMapSummaryCorrectly()
    {
        // Arrange
        var customer = CustomerFactory.CreateCustomer().Value;
        var labor = EmployeeFactory.CreateLabor().Value;
        var vehicle = customer.Vehicles.First();

        var repairTask = RepairTaskFactory.CreateRepairTask(name: "Oil Change").Value;

        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: vehicle.Id, laborId: labor.Id, repairTasks: [repairTask]).Value;
        workOrder.Vehicle = vehicle;
        workOrder.Labor = labor;

        // Act
        var dto = workOrder.ToListItemDto();

        // Assert
        Assert.Equal(workOrder.Id, dto.WorkOrderId);
        Assert.Equal(workOrder.Spot, dto.Spot);
        Assert.Equal(workOrder.StartAtUtc, dto.StartAtUtc);
        Assert.Equal(workOrder.EndAtUtc, dto.EndAtUtc);
        Assert.Equal(vehicle.Make, dto.Vehicle.Make);
        Assert.Equal($"{labor.FirstName} {labor.LastName}", dto.Labor);
        Assert.Single(dto.RepairTasks);
        Assert.Equal("Oil Change", dto.RepairTasks[0]);
        Assert.Equal(workOrder.State, dto.State);
    }
}