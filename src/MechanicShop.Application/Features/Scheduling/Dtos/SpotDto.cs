using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.Scheduling.Dtos;

public sealed class SpotDto
{
    public Spot Spot { get; init; }
    public List<AvailabilitySlotDto> Slots { get; init; } = [];
}