using MechanicShop.Contracts.Common;

namespace MechanicShop.Client.Models;

public class SpotModel
{
    public Spot Spot { get; set; }
    public List<AvailabilitySlotModel> Slots { get; set; } = [];
}