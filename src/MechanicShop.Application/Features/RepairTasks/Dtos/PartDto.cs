namespace MechanicShop.Application.Features.RepairTasks.Dtos;

public sealed record PartDto
{
    public Guid PartId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Cost { get; init; }
    public int Quantity { get; init; }
}