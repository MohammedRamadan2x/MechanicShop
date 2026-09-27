using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.RepairTasks;

public static class RepairTaskErrors
{
    public static Error IdRequired =>
        Error.Validation("RepairTask.IdRequired", "Repair task ID is required.");

    public static Error NameRequired =>
        Error.Validation("RepairTask.NameRequired", "Name is required.");

    public static Error InvalidLaborCost =>
        Error.Validation("RepairTask.InvalidLaborCost", 
            "Labor cost must be between 1 and 10,000.");

    public static Error InvalidDuration =>
        Error.Validation("RepairTask.InvalidDuration", "Invalid duration selected.");

    public static Error PartsRequired =>
        Error.Validation("RepairTask.PartsRequired", "At least one part is required.");

    public static Error InvalidPart =>
        Error.Validation("RepairTask.InvalidPart",
            "Parts collection contains an invalid part.");

    public static Error PartNameRequired =>
        Error.Validation("RepairTask.PartNameRequired", "All parts must have a name.");

    public static Error AtLeastOneRepairTaskIsRequired =>
        Error.Validation("RepairTask.Required", 
            "At least one repair task must be specified.");

    public static Error InUse =>
        Error.Conflict("RepairTask.InUse", 
            "Cannot delete a repair task that is used in work orders.");

    public static Error DuplicatePartName =>
        Error.Conflict("RepairTask.DuplicatePartName", 
            "A part with the same name already exists in this repair task.");

    public static Error DuplicateName =>
        Error.Conflict("RepairTask.DuplicateName",
            "A repair task with the same name already exists.");
}