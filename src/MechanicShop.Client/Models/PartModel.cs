namespace MechanicShop.Client.Models;

public class PartModel
{
    public Guid PartId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public int Quantity { get; set; }
}