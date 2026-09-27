using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Domain.WorkOrders;

public static class WorkOrderErrors
{
    public static Error WorkOrderIdRequired => 
        Error.Validation("WorkOrder.IdRequired", "Work order ID is required");

    public static Error VehicleIdRequired => 
        Error.Validation("WorkOrder.VehicleIdRequired", "Vehicle ID is required");

    public static Error LaborIdRequired => 
        Error.Validation("WorkOrder.LaborIdRequired", "Labor ID is required");

    public static Error RepairTasksRequired => 
        Error.Validation("WorkOrder.RepairTasksRequired", 
            "At least one repair task is required");

    public static Error InvalidRepairTask =>
        Error.Validation("WorkOrder.InvalidRepairTask",
            "Repair tasks collection contains an invalid repair task.");

    public static Error RepairTaskCannotBeNull =>
        Error.Validation("WorkOrder.RepairTaskCannotBeNull", 
            "Repair task cannot be null.");

    public static Error InvalidSpot => 
        Error.Validation("WorkOrder.InvalidSpot", "The provided spot is invalid");

    public static Error InvalidState =>
        Error.Validation("WorkOrder.InvalidState", "Invalid State selected.");

    public static Error LaborIdEmpty(string id) => 
        Error.Validation("WorkOrder.LaborIdEmpty",
            $"Labor ID is required for work order '{id}'.");

    public static Error InvalidTiming => 
        Error.Conflict("WorkOrder.InvalidTiming", "End time must be after start time.");

    public static Error ReadOnly => 
        Error.Conflict("WorkOrder.ReadOnly", "This work order is read-only.");

    public static Error TimingReadOnly(string id, WorkOrderState state) => 
        Error.Conflict("WorkOrder.TimingReadOnly",
            $"Cannot modify timing for work order '{id}' when its state is '{state}'.");

    public static Error StateTransitionNotAllowed(DateTimeOffset startAtUtc) => 
        Error.Conflict("WorkOrder.StateTransitionNotAllowed",
            $"State transition is not allowed before the work order’s scheduled " +
            $"start time {startAtUtc:yyyy-MM-dd HH:mm} UTC.");

    public static Error InvalidStateTransition(
        WorkOrderState current, 
        WorkOrderState next) => 
        Error.Conflict("WorkOrder.InvalidStateTransition",
            $"Invalid work order state transition from '{current}' to '{next}'.");

    public static Error RepairTaskAlreadyAdded => 
        Error.Conflict("WorkOrder.RepairTaskAlreadyAdded",
            "This repair task has already been added.");

    public static Error InvalidStateTransitionTime => 
        Error.Conflict("WorkOrder.InvalidStateTransitionTime",
            "State transition is not allowed before the work order’s scheduled" +
            " start time.");
}