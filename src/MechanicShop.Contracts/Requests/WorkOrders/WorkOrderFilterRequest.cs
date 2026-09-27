using MechanicShop.Contracts.Common;

namespace MechanicShop.Contracts.Requests.WorkOrders;

public record WorkOrderFilterRequest
{
    public string? SearchTerm { get; set; }
    public string SortColumn { get; set; } = "createdAt";
    public string SortDirection { get; set; } = "desc";
    public WorkOrderState? State { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? LaborId { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
    public Spot? Spot { get; set; }
}