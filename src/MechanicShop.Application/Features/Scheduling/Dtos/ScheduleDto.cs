namespace MechanicShop.Application.Features.Scheduling.Dtos;

public sealed class ScheduleDto
{
    public DateOnly OnDate { get; init; }
    public bool EndOfDay { get; init; }
    public List<SpotDto> Spots { get; init; } = [];
}