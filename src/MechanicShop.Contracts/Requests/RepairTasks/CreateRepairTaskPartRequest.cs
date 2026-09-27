using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.RepairTasks;

public class CreateRepairTaskPartRequest
{
    [Required(ErrorMessage = "Part name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cost is required.")]
    [Range(1, 10000, ErrorMessage = "Cost must be between 1 and 10,000.")]
    public decimal Cost { get; set; }

    [Required(ErrorMessage = "Quantity is required.")]
    [Range(1, 10, ErrorMessage = "Quantity must be between 1 and 10.")]
    public int Quantity { get; set; }
}