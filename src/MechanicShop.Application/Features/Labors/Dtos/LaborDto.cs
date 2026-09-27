namespace MechanicShop.Application.Features.Labors.Dtos;

public sealed record LaborDto
{
    public Guid LaborId { get; init; }
    public string Name { get; init; } = string.Empty;
}