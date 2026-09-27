namespace MechanicShop.Client.Models;

public class ScheduleModel
{
    public DateOnly OnDate { get; set; }
    public bool EndOfDay { get; set; }
    public List<SpotModel> Spots { get; set; } = [];
}