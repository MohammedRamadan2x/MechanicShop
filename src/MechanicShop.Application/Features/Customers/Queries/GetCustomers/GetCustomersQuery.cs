using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomers;

public sealed record GetCustomersQuery : ICachedQuery<Result<List<CustomerDto>>>
{
    public string CacheKey => "customers";

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);

    public string[] Tags => ["customer"];
}