using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.CreateWorkOrder;

public class CreateWorkOrderCommandValidatorTests
{
    private readonly CreateWorkOrderCommandValidator _validator;

    public CreateWorkOrderCommandValidatorTests()
    {
        _validator = new CreateWorkOrderCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_VehicleId_Is_Empty()
    {
        var command = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: Guid.Empty,
            StartAt: DateTime.UtcNow.AddHours(1),
            RepairTaskIds: [Guid.NewGuid()],
            LaborId: Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "VehicleId");
    }

    [Fact]
    public void Should_Have_Error_When_StartAt_Is_Not_In_Future()
    {
        var command = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: Guid.Empty,
            StartAt: DateTime.UtcNow.AddSeconds(-1),
            RepairTaskIds: [Guid.NewGuid()],
            LaborId: Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StartAt");
    }

    [Fact]
    public void Should_Have_Error_When_RepairTaskIds_Is_Empty()
    {
        var command = new CreateWorkOrderCommand(
            Spot: Spot.A,
            VehicleId: Guid.NewGuid(),
            StartAt: DateTime.UtcNow.AddHours(1),
            RepairTaskIds: [],
            LaborId: Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RepairTaskIds");
    }

    [Fact]
    public void Should_Have_Error_When_LaborId_Is_EmptyGuid()
    {
        var command = new CreateWorkOrderCommand(
             Spot: Spot.A,
             VehicleId: Guid.Empty,
             StartAt: DateTime.UtcNow.AddHours(1),
             RepairTaskIds: [Guid.NewGuid()],
             LaborId: Guid.Empty);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LaborId");
    }

    [Fact]
    public void Should_Have_Error_When_Spot_Is_Invalid()
    {
        var command = new CreateWorkOrderCommand(
           Spot: (Spot)999,
           VehicleId: Guid.NewGuid(),
           StartAt: DateTime.UtcNow.AddHours(1),
           RepairTaskIds: [Guid.NewGuid()],
           LaborId: Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Spot");
    }

    [Fact]
    public void Should_Pass_When_Valid()
    {
        var command = new CreateWorkOrderCommand(
           Spot: Spot.A,
           VehicleId: Guid.NewGuid(),
           StartAt: DateTime.UtcNow.AddHours(1),
           RepairTaskIds: [Guid.NewGuid()],
           LaborId: Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}