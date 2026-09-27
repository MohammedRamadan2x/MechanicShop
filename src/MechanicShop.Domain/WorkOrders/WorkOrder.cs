using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Domain.WorkOrders;

public sealed class WorkOrder : AuditableEntity
{
    public Guid VehicleId { get; }
    public Guid LaborId { get; private set; }

    public Employee? Labor { get; set; }  // Navigation Property 
    public Vehicle? Vehicle { get; set; } // Navigation Property
    public Invoice? Invoice { get; set; } // Navigation Property

    public DateTimeOffset StartAtUtc { get; private set; }
    public DateTimeOffset EndAtUtc { get; private set; }

    public Spot Spot { get; private set; }
    public WorkOrderState State { get; private set; }
    
    public decimal? Discount { get; private set; }
    public decimal? Tax { get; private set; }

    public decimal? TotalPartsCost => _repairTasks
        .SelectMany(rt => rt.Parts)
        .Sum(p => p.Cost * p.Quantity);
    public decimal? TotalLaborCost => _repairTasks.Sum(rt => rt.LaborCost);
    public decimal? Total => (TotalPartsCost ?? 0) + (TotalLaborCost ?? 0);


    private readonly List<RepairTask> _repairTasks = [];
    public IEnumerable<RepairTask> RepairTasks => _repairTasks.AsReadOnly();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private WorkOrder()
    { }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private WorkOrder(
        Guid id, 
        Guid vehicleId, 
        DateTimeOffset startAt, 
        DateTimeOffset endAt, 
        Guid laborId, 
        Spot spot, 
        WorkOrderState state, 
        List<RepairTask> repairTasks) : base(id)
    {
        VehicleId = vehicleId;
        StartAtUtc = startAt;
        EndAtUtc = endAt;
        LaborId = laborId;
        Spot = spot;
        State = state;
        _repairTasks = new List<RepairTask>(repairTasks);
    }

    public static Result<WorkOrder> Create(
        Guid id, 
        Guid vehicleId, 
        DateTimeOffset startAt, 
        DateTimeOffset endAt, 
        Guid laborId, 
        Spot spot, 
        List<RepairTask> repairTasks)
    {
        if (id == Guid.Empty)
        {
            return WorkOrderErrors.WorkOrderIdRequired;
        }

        if (vehicleId == Guid.Empty)
        {
            return WorkOrderErrors.VehicleIdRequired;
        }

        if (repairTasks == null || repairTasks.Count == 0)
        {
            return WorkOrderErrors.RepairTasksRequired;
        }

        if (repairTasks.Any(rt => rt is null))
        {
            return WorkOrderErrors.InvalidRepairTask;
        }

        if (laborId == Guid.Empty)
        {
            return WorkOrderErrors.LaborIdRequired;
        }

        if (endAt <= startAt)
        {
            return WorkOrderErrors.InvalidTiming;
        }

        if (!Enum.IsDefined(spot))
        {
            return WorkOrderErrors.InvalidSpot;
        }

        return new WorkOrder(
            id, 
            vehicleId, 
            startAt, 
            endAt, 
            laborId, 
            spot, 
            WorkOrderState.Scheduled, 
            repairTasks);
    }

    public Result<Updated> AddRepairTask(RepairTask repairTask)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.ReadOnly;
        }

        if (repairTask is null)
        {
            return WorkOrderErrors.RepairTaskCannotBeNull;
        }

        if (_repairTasks.Any(r => r.Id == repairTask.Id))
        {
            return WorkOrderErrors.RepairTaskAlreadyAdded;
        }

        _repairTasks.Add(repairTask);

        return Result.Updated;
    }

    public Result<Updated> UpdateTiming(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.TimingReadOnly(Id.ToString(), State);
        }

        if (endAt <= startAt)
        {
            return WorkOrderErrors.InvalidTiming;
        }

        StartAtUtc = startAt;
        EndAtUtc = endAt;

        return Result.Updated;
    }

    public Result<Updated> UpdateLabor(Guid laborId)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.ReadOnly;
        }

        if (laborId == Guid.Empty)
        {
            return WorkOrderErrors.LaborIdEmpty(Id.ToString());
        }

        LaborId = laborId;

        return Result.Updated;
    }

    public Result<Updated> UpdateState(WorkOrderState newState)
    {
        if (!Enum.IsDefined(newState))
        {
            return WorkOrderErrors.InvalidState;
        }

        if (!CanTransitionTo(newState))
        {
            return WorkOrderErrors.InvalidStateTransition(State, newState);
        }

        State = newState;

        return Result.Updated;
    }

    public bool IsEditable => 
        State is not (
        WorkOrderState.Completed or 
        WorkOrderState.Cancelled or 
        WorkOrderState.InProgress);

    public bool CanTransitionTo(WorkOrderState newState)
    {
        return (State, newState) switch
        {
            (WorkOrderState.Scheduled, WorkOrderState.InProgress) => true,
            (WorkOrderState.InProgress, WorkOrderState.Completed) => true,
            (_, WorkOrderState.Cancelled) when State != WorkOrderState.Completed => true,
            _ => false
        };
    }

    public Result<Updated> Cancel()
    {
        if (!CanTransitionTo(WorkOrderState.Cancelled))
        {
            return WorkOrderErrors.InvalidStateTransition(
                State, 
                WorkOrderState.Cancelled);
        }

        State = WorkOrderState.Cancelled;

        return Result.Updated;
    }

    public Result<Updated> ClearRepairTasks()
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.ReadOnly;
        }

        _repairTasks.Clear();

        return Result.Updated;
    }

    public Result<Updated> UpdateSpot(Spot newSpot)
    {
        if (!IsEditable)
        {
            return WorkOrderErrors.ReadOnly;
        }

        if (!Enum.IsDefined(newSpot))
        {
            return WorkOrderErrors.InvalidSpot;
        }

        Spot = newSpot;

        return Result.Updated;
    }
}