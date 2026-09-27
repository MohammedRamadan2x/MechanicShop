using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;

using Xunit;

namespace MechanicShop.Domain.UnitTests.WorkOrders;

public class WorkOrderTests
{
    [Fact]
    public void CreateWorkOrder_ShouldSucceed_WhenEmptyId()
    {
        var workOrderId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var startAt = DateTimeOffset.UtcNow;
        var endAt = DateTimeOffset.UtcNow.AddHours(1);
        var laborId = Guid.NewGuid();
        var spot = Spot.A;
        List<RepairTask> repairTasks = [RepairTaskFactory.CreateRepairTask().Value];

        var result = WorkOrder.Create(
            workOrderId,
            vehicleId,
            startAt,
            endAt,
            laborId,
            spot,
            repairTasks);

        Assert.True(result.IsSuccess);

        var workOrder = result.Value;

        Assert.IsType<WorkOrder>(workOrder);
        Assert.NotNull(workOrder);
        Assert.Single(workOrder.RepairTasks);
        Assert.Equal(workOrderId, workOrder.Id);
        Assert.Equal(vehicleId, workOrder.VehicleId);
        Assert.Equal(startAt, workOrder.StartAtUtc);
        Assert.Equal(endAt, workOrder.EndAtUtc);
        Assert.Equal(laborId, workOrder.LaborId);
        Assert.Equal(spot, workOrder.Spot);
        Assert.Equal(repairTasks, workOrder.RepairTasks);
        Assert.Equal(WorkOrderState.Scheduled, workOrder.State);
    }

    [Fact]
    public void CreateWorkOrder_ShouldFail_WhenEmptyId()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder(id: Guid.Empty);

        Assert.False(workOrder.IsSuccess);

        Assert.Equal(WorkOrderErrors.WorkOrderIdRequired.Code, workOrder.TopError.Code);
    }

    [Fact]
    public void CreateWorkOrder_ShouldFail_WhenEmptyVehicleId()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder(vehicleId: Guid.Empty);

        Assert.False(workOrder.IsSuccess);

        Assert.Equal(WorkOrderErrors.VehicleIdRequired.Code, workOrder.TopError.Code);
    }

    [Fact]
    public void CreateWorkOrder_ShouldFail_WhenNoRepairTasks()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder(repairTasks: []);

        Assert.False(workOrder.IsSuccess);

        Assert.Equal(WorkOrderErrors.RepairTasksRequired.Code, workOrder.TopError.Code);
    }

    [Fact]
    public void CreateWorkOrder_ShouldFail_WhenEmptyLaborId()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder(laborId: Guid.Empty);

        Assert.False(workOrder.IsSuccess);

        Assert.Equal(WorkOrderErrors.LaborIdRequired.Code, workOrder.TopError.Code);
    }

    [Fact]
    public void CreateWorkOrder_ShouldFail_WhenInvalidTiming()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder(
            startAt: DateTimeOffset.UtcNow.AddHours(1),
            endAt: DateTimeOffset.UtcNow);

        Assert.False(workOrder.IsSuccess);

        Assert.Equal(WorkOrderErrors.InvalidTiming.Code, workOrder.TopError.Code);
    }

    [Fact]
    public void CreateWorkOrder_ShouldFail_WhenInvalidSpot()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder(spot: (Spot)999);

        Assert.False(workOrder.IsSuccess);

        Assert.Equal(WorkOrderErrors.InvalidSpot.Code, workOrder.TopError.Code);
    }
    
    [Fact]
    public void AddRepairTask_ShouldFail_WhenNotEditable()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.UpdateState(WorkOrderState.InProgress);
        workOrder.UpdateState(WorkOrderState.Completed);

        var result = workOrder.AddRepairTask(
            RepairTaskFactory.CreateRepairTask().Value);

        Assert.False(result.IsSuccess);

        Assert.Equal(WorkOrderErrors.ReadOnly.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateLabor_ShouldFail_WhenLaborIdEmpty()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        var result = workOrder.UpdateLabor(Guid.Empty);

        Assert.False(result.IsSuccess);

        Assert.Equal(
            WorkOrderErrors.LaborIdEmpty(workOrder.Id.ToString()).Code, 
            result.TopError.Code);
    }

    [Fact]
    public void UpdateSpot_ShouldFail_WhenInvalidSpot()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        var result = workOrder.UpdateSpot((Spot)999);

        Assert.False(result.IsSuccess);

        Assert.Equal(WorkOrderErrors.InvalidSpot.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateTiming_ShouldFail_WhenInvalidTiming()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        var result = workOrder.UpdateTiming(
            DateTimeOffset.UtcNow.AddHours(2), 
            DateTimeOffset.UtcNow);

        Assert.False(result.IsSuccess);

        Assert.Equal(WorkOrderErrors.InvalidTiming.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateState_ShouldFail_WhenInvalidTransition()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        var result = workOrder.UpdateState(WorkOrderState.Completed);

        Assert.False(result.IsSuccess);

        Assert.Equal(
            WorkOrderErrors.InvalidStateTransition(
                WorkOrderState.Scheduled,
                WorkOrderState.Completed).Code, 
            result.TopError.Code);
    }

    [Fact]
    public void UpdateLabor_ShouldSucceed_AndSetNewLaborId()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        var newLabor = Guid.NewGuid();

        var result = workOrder.UpdateLabor(newLabor);

        Assert.True(result.IsSuccess);
        Assert.Equal(newLabor, workOrder.LaborId);
    }

    [Fact]
    public void UpdateSpot_ShouldSucceed_AndSetNewSpot()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        var result = workOrder.UpdateSpot(Spot.B);

        Assert.True(result.IsSuccess);
        Assert.Equal(Spot.B, workOrder.Spot);
    }

    [Fact]
    public void UpdateTiming_ShouldSucceed_AndSetNewTiming()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        var newStart = workOrder.StartAtUtc.AddHours(2);
        var newEnd = newStart.AddHours(1);

        var result = workOrder.UpdateTiming(newStart, newEnd);

        Assert.True(result.IsSuccess);
        Assert.Equal(newStart, workOrder.StartAtUtc);
        Assert.Equal(newEnd, workOrder.EndAtUtc);
    }

    [Fact]
    public void UpdateState_ShouldSucceed_AndSetStateToInProgress()
    {
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        var result = workOrder.UpdateState(WorkOrderState.InProgress);

        Assert.True(result.IsSuccess);
        Assert.Equal(WorkOrderState.InProgress, workOrder.State);
    }
}