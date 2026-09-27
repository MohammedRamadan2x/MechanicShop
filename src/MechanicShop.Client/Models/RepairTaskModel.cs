using MechanicShop.Contracts.Common;

namespace MechanicShop.Client.Models;

public class RepairTaskModel
{
    public Guid RepairTaskId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal LaborCost { get; set; }
    public RepairDurationInMinutes EstimatedDurationInMins { get; set; }
    public decimal TotalCost { get; set; }
    public List<PartModel> Parts { get; set; } = [];
}