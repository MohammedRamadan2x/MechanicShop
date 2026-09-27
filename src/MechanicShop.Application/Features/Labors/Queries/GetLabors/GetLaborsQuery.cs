using MechanicShop.Application.Features.Labors.Dtos;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Labors.Queries.GetLabors;

public sealed record GetLaborsQuery : ICachedQuery<Result<List<LaborDto>>>
{
    public string CacheKey => $"labors";
    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
    public string[] Tags => ["labors"];
}