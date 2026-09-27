using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Scheduling.Queries.GetDailySchedule;

public sealed record GetDailyScheduleQuery(
    TimeZoneInfo TimeZone,
    DateOnly ScheduleDate,
    Guid? LaborId = null) 
    : ICachedQuery<Result<ScheduleDto>>
{
    public string CacheKey => $"work-order:{ScheduleDate:yyyy-MM-dd}" +
        $":labor={LaborId?.ToString() ?? "-"}";

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);

    public string[] Tags => ["work-order"];
}