using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.RepairTasks.Parts;

public static class PartErrors
{
    public static Error IdRequired =>
        Error.Validation("Part.IdRequired", "Part ID is required.");

    public static Error NameRequired =>
        Error.Validation("Part.NameRequired", "Part name is required.");

    public static Error InvalidCost =>
        Error.Validation("Part.InvalidCost", "Part cost must be between 1 and 10,000.");

    public static Error InvalidQuantity =>
        Error.Validation("Part.InvalidQuantity", "Quantity must be between 1 and 10.");
}
