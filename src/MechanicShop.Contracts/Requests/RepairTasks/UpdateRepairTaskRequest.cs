using System.ComponentModel.DataAnnotations;

using MechanicShop.Contracts.Common;

namespace MechanicShop.Contracts.Requests.RepairTasks;

public class UpdateRepairTaskRequest
{
    [Required(ErrorMessage = "Task name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Labor cost is required.")]
    [Range(1, 10000, ErrorMessage = "Labor cost must be between 1 and 10,000.")]
    public decimal LaborCost { get; set; }

    [Required(ErrorMessage = "Estimated duration is required.")]
    public RepairDurationInMinutes EstimatedDurationInMins { get; set; }

    [MinLength(1, ErrorMessage = "At least one part is required.")]
    public List<UpdateRepairTaskPartRequest> Parts { get; set; } = [];
}