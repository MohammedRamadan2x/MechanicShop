using MechanicShop.Contracts.Common;

namespace MechanicShop.Contracts.Requests.WorkOrders;

public class RelocateWorkOrderRequest
{
    public DateTimeOffset NewStartAtUtc { get; set; }
    public Spot NewSpot { get; set; }
}