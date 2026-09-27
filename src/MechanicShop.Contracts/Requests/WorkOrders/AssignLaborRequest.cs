using System.ComponentModel.DataAnnotations;

namespace MechanicShop.Contracts.Requests.WorkOrders;

public class AssignLaborRequest
{
    [Required(ErrorMessage = "LaborId is required.")]
    public Guid LaborId { get; set; }
}