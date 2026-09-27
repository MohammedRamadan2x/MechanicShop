using System.ComponentModel.DataAnnotations;

using MechanicShop.Contracts.Common;

namespace MechanicShop.Contracts.Requests.WorkOrders;

public class CreateWorkOrderRequest
{
    public Guid? LaborId { get; set; }

    [Required(ErrorMessage = "Spot is required.")]
    [Range(0, 3, ErrorMessage = "Invalid range [0, 1, 2 or 3]")]
    public Spot Spot { get; set; }

    [Required(ErrorMessage = "Vehicle is required.")]
    public Guid VehicleId { get; set; }

    [MinLength(1, ErrorMessage = "At least one repair task must be selected.")]
    public List<Guid> RepairTaskIds { get; set; } = [];

    [Required(ErrorMessage = "StartAt is required.")]
    public DateTimeOffset StartAtUtc { get; set; }
}