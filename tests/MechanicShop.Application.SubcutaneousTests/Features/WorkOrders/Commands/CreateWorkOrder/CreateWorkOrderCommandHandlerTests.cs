using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;

using MediatR;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.CreateWorkOrder;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CreateWorkOrderCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
        // Arrange
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Vehicles.AddAsync(vehicle);
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date
            .AddDays(1)
            .AddHours(10);

        var command = new CreateWorkOrderCommand(
            Spot: Spot.B,
            VehicleId: vehicle.Id,
            StartAt: scheduledAt,
            RepairTaskIds: [repairTask.Id],
            LaborId: employee.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(vehicle.Id, result.Value.Vehicle!.VehicleId);
        Assert.Equal(employee.Id, result.Value.Labor!.LaborId);
        Assert.Equal(Spot.B, result.Value.Spot);
        Assert.Single(result.Value.RepairTasks);
        Assert.Equal(repairTask.Id, result.Value.RepairTasks[0].RepairTaskId);
    }

    [Fact]
    public async Task Handle_WithMissingRepairTask_ShouldFail()
    {
        // Arrange
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Vehicles.AddAsync(vehicle);
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync(default);

        var fakeRepairTaskId = Guid.NewGuid();

        var scheduledAt = DateTimeOffset.UtcNow.Date
        .AddDays(1)
        .AddHours(11);

        var command = new CreateWorkOrderCommand(
            Spot: Spot.C,
            VehicleId: vehicle.Id,
            StartAt: scheduledAt,
            RepairTaskIds: [fakeRepairTaskId],
            LaborId: employee.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Handle_WithOutsideOperatingHours_ShouldFail()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;
        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Vehicles.AddAsync(vehicle);
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date
             .AddDays(1)
             .AddHours(4);

        var command = new CreateWorkOrderCommand(Spot.B, vehicle.Id, scheduledAt, [repairTask.Id], employee.Id);
        var result = await _mediator.Send(command);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Handle_WithShortDuration_ShouldFail()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min15).Value;
        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Vehicles.AddAsync(vehicle);
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date
         .AddDays(1)
         .AddHours(12);

        var command = new CreateWorkOrderCommand(Spot.A, vehicle.Id, scheduledAt, [repairTask.Id], employee.Id);
        var result = await _mediator.Send(command);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrder.TooShort");
    }

    [Fact]
    public async Task Handle_WithMissingVehicle_ShouldFail()
    {
        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;
        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.RepairTasks.AddAsync(repairTask);
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date
              .AddDays(1)
              .AddHours(13);

        var command = new CreateWorkOrderCommand(Spot.C, Guid.NewGuid(), scheduledAt, [repairTask.Id], employee.Id);
        var result = await _mediator.Send(command);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Handle_WithMissingLabor_ShouldFail()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;

        await _context.Customers.AddAsync(customer);
        await _context.Vehicles.AddAsync(vehicle);
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date
             .AddDays(1)
             .AddHours(14);

        var command = new CreateWorkOrderCommand(Spot.C, vehicle.Id, scheduledAt, [repairTask.Id], Guid.NewGuid());
        var result = await _mediator.Send(command);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Handle_WithVehicleConflict_ShouldFail()
    {
        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;
        var employee1 = EmployeeFactory.CreateEmployee().Value;
        var employee2 = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Vehicles.AddAsync(vehicle);
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.Employees.AddAsync(employee1);
        await _context.Employees.AddAsync(employee2);
        await _context.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date
           .AddDays(1)
           .AddHours(15);

        var command1 = new CreateWorkOrderCommand(Spot.A, vehicle.Id, scheduledAt, [repairTask.Id], employee1.Id);
        var command2 = new CreateWorkOrderCommand(Spot.B, vehicle.Id, scheduledAt, [repairTask.Id], employee2.Id);

        await _mediator.Send(command1);
        var result = await _mediator.Send(command2);

        Assert.True(result.IsFailure);
        Assert.Equal("Vehicle.Overlapping.WorkOrders", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WithLaborConflict_ShouldFail()
    {
        var customer1 = CustomerFactory.CreateCustomer().Value;
        var vehicle1 = customer1.Vehicles.First();
        var customer2 = CustomerFactory.CreateCustomer().Value;
        var vehicle2 = customer2.Vehicles.First();

        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;
        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer1);
        await _context.Customers.AddAsync(customer2);
        await _context.Vehicles.AddAsync(vehicle1);
        await _context.Vehicles.AddAsync(vehicle2);
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date
                  .AddDays(1)
                  .AddHours(16);

        var command1 = new CreateWorkOrderCommand(Spot.A, vehicle1.Id, scheduledAt, [repairTask.Id], employee.Id);
        var command2 = new CreateWorkOrderCommand(Spot.B, vehicle2.Id, scheduledAt, [repairTask.Id], employee.Id);

        await _mediator.Send(command1);
        var result = await _mediator.Send(command2);

        Assert.True(result.IsFailure);
        Assert.Equal("Labor.Occupied", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_WithUnavailableSpot_ShouldFail()
    {
        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;

        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;

        var repairTask = RepairTaskFactory.CreateRepairTask(repairDurationInMinutes: RepairDurationInMinutes.Min60).Value;
        var employee1 = EmployeeFactory.CreateEmployee().Value;
        var employee2 = EmployeeFactory.CreateEmployee().Value;

        await _context.Customers.AddAsync(customer);
        await _context.Vehicles.AddAsync(vehicle1);
        await _context.Vehicles.AddAsync(vehicle2);
        await _context.RepairTasks.AddAsync(repairTask);
        await _context.Employees.AddAsync(employee1);
        await _context.Employees.AddAsync(employee2);
        await _context.SaveChangesAsync(default);

        var scheduledAt = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(17);

        var command1 = new CreateWorkOrderCommand(Spot.A, vehicle1.Id, scheduledAt, [repairTask.Id], employee1.Id);
        var command2 = new CreateWorkOrderCommand(Spot.A, vehicle2.Id, scheduledAt, [repairTask.Id], employee2.Id);

        await _mediator.Send(command1);
        var result = await _mediator.Send(command2);

        Assert.True(result.IsFailure);
    }
}