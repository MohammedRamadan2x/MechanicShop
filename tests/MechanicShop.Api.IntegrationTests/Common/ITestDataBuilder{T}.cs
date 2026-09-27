using MechanicShop.Tests.Common.Security;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Api.IntegrationTests.Common;

public interface ITestDataBuilder<T>
{
    T Build();
}

public class WorkOrderTestDataBuilder : ITestDataBuilder<WorkOrder>
{
    private Guid _id = Guid.NewGuid();
    private Guid _vehicleId = Guid.NewGuid();
    private DateTimeOffset _startAt = DateTimeOffset.UtcNow;
    private DateTimeOffset _endAt = DateTimeOffset.UtcNow.AddHours(2);
    private Guid _laborId = Guid.Parse(TestUsers.Labor01.Id);
    private Spot _spot = Spot.A;
    private List<RepairTask> _repairTasks = [];
    private WorkOrderState _state = WorkOrderState.Scheduled;

    public static WorkOrderTestDataBuilder Create() => new();

    public WorkOrderTestDataBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public WorkOrderTestDataBuilder WithVehicle(Guid vehicleId)
    {
        _vehicleId = vehicleId;
        return this;
    }

    public WorkOrderTestDataBuilder WithTimeSlot(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        _startAt = startAt;
        _endAt = endAt;
        return this;
    }

    public WorkOrderTestDataBuilder WithLabor(string laborId)
    {
        _laborId = Guid.Parse(laborId);
        return this;
    }

    public WorkOrderTestDataBuilder WithLabor(Guid laborId)
    {
        _laborId = laborId;
        return this;
    }

    public WorkOrderTestDataBuilder AtSpot(Spot spot)
    {
        _spot = spot;
        return this;
    }

    public WorkOrderTestDataBuilder WithRepairTasks(params RepairTask[] repairTasks)
    {
        _repairTasks = [.. repairTasks];
        return this;
    }

    public WorkOrderTestDataBuilder WithRepairTasks(List<RepairTask> repairTasks)
    {
        _repairTasks = repairTasks;
        return this;
    }

    public WorkOrderTestDataBuilder WithState(WorkOrderState state)
    {
        _state = state;
        return this;
    }

    public WorkOrderTestDataBuilder ForToday(TimeOnly? from = null, TimeOnly? to = null)
    {
        var today = DateTimeOffset.UtcNow.Date;

        var fromTime = from ?? new TimeOnly(9, 0);
        var toTime = to ?? new TimeOnly(11, 0);

        _startAt = today.Add(fromTime.ToTimeSpan());
        _endAt = today.Add(toTime.ToTimeSpan());

        return this;
    }

    public WorkOrderTestDataBuilder InProgress()
    {
        _state = WorkOrderState.InProgress;
        _startAt = DateTimeOffset.UtcNow.AddMinutes(-30);
        return this;
    }

    public WorkOrderTestDataBuilder Completed()
    {
        _state = WorkOrderState.Completed;
        _startAt = DateTimeOffset.UtcNow.AddHours(-3);
        _endAt = DateTimeOffset.UtcNow.AddHours(-1);
        return this;
    }

    public WorkOrder Build()
    {
        var workOrder = WorkOrder.Create(
            id: _id,
            vehicleId: _vehicleId,
            startAt: _startAt,
            endAt: _endAt,
            laborId: _laborId,
            spot: _spot,
            repairTasks: _repairTasks).Value;

        // Set state if different from default
        if (_state != WorkOrderState.Scheduled)
        {
            // Assuming you have a method to set state
            workOrder.UpdateState(_state);
        }

        return workOrder;
    }
}