using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.RepairTasks.Parts;

namespace MechanicShop.Domain.RepairTasks;

public sealed class RepairTask : AuditableEntity
{
    public string Name { get; private set; }
    public decimal LaborCost { get; private set; }
    public RepairDurationInMinutes EstimatedDurationInMins { get; private set; }

    private readonly List<Part> _parts = [];
    public IEnumerable<Part> Parts => _parts.AsReadOnly();
    public decimal TotalCost => LaborCost + Parts.Sum(p => p.Cost * p.Quantity);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private RepairTask()
    { }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private RepairTask(
        Guid id, 
        string name, 
        decimal laborCost, 
        RepairDurationInMinutes estimatedDurationInMins, 
        List<Part> parts) : base(id)
    {
        Name = name;
        LaborCost = laborCost;
        EstimatedDurationInMins = estimatedDurationInMins;
        _parts = new List<Part>(parts);
    }

    public static Result<RepairTask> Create(
        Guid id, 
        string name, 
        decimal laborCost, 
        RepairDurationInMinutes estimatedDurationInMins, 
        List<Part> parts)
    {
        if (id == Guid.Empty)
        {
            return RepairTaskErrors.IdRequired;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return RepairTaskErrors.NameRequired;
        }

        if (laborCost <= 0 || laborCost > 10000)
        {
            return RepairTaskErrors.InvalidLaborCost;
        }

        if (!Enum.IsDefined(estimatedDurationInMins))
        {
            return RepairTaskErrors.InvalidDuration;
        }

        if (parts is null || parts.Count == 0)
        {
            return RepairTaskErrors.PartsRequired;
        }

        if (parts.Any(p => p is null))
        {
            return RepairTaskErrors.InvalidPart;
        }

        if (parts
            .GroupBy(p => p.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Any(g => g.Count() > 1))
        {
            return RepairTaskErrors.DuplicatePartName;
        }

        return new RepairTask(
            id, 
            name.Trim(), 
            laborCost, 
            estimatedDurationInMins, 
            parts);
    }

    public Result<Updated> Update(
        string name, 
        decimal laborCost, 
        RepairDurationInMinutes estimatedDurationInMins)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return RepairTaskErrors.NameRequired;
        }

        if (laborCost <= 0 || laborCost > 10000)
        {
            return RepairTaskErrors.InvalidLaborCost;
        }

        if (!Enum.IsDefined(estimatedDurationInMins))
        {
            return RepairTaskErrors.InvalidDuration;
        }

        Name = name.Trim();
        LaborCost = laborCost;
        EstimatedDurationInMins = estimatedDurationInMins;

        return Result.Updated;
    }

    public Result<Updated> UpsertParts(List<Part> incomingParts)
    {
        if (incomingParts is null || incomingParts.Count == 0)
        {
            return RepairTaskErrors.PartsRequired;
        }

        if (incomingParts.Any(p => p is null))
        {
            return RepairTaskErrors.InvalidPart;
        }

        if (incomingParts
            .GroupBy(p => p.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Any(g => g.Count() > 1))
        {
            return RepairTaskErrors.DuplicatePartName;
        }

        _parts.RemoveAll(existing => incomingParts.All(p => p.Id != existing.Id));

        foreach (var incoming in incomingParts)
        {
            var existing = _parts.FirstOrDefault(p => p.Id == incoming.Id);

            if (existing is null)
            {
                _parts.Add(incoming);
            }
            else
            {
                var updatePartResult = existing.Update(
                    incoming.Name, 
                    incoming.Cost, 
                    incoming.Quantity);
                
                if (updatePartResult.IsFailure)
                {
                    return updatePartResult.Errors;
                }
            }
        }

        return Result.Updated;
    }
}