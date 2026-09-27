using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;

namespace MechanicShop.Tests.Common.RepairTasks;

public static class RepairTaskFactory
{
    public static Result<RepairTask> CreateRepairTask(
        Guid? id = null,
        string? name = null,
        decimal? laborCost = null,
        RepairDurationInMinutes? repairDurationInMinutes = null,
        List<Part>? parts = null)
    {
        return RepairTask.Create(
            id ?? Guid.NewGuid(),
            name ?? "Brake Inspection",
            laborCost ?? 100,
            repairDurationInMinutes ?? RepairDurationInMinutes.Min30,
            parts ?? [PartFactory.CreatePart().Value]);
    }
}