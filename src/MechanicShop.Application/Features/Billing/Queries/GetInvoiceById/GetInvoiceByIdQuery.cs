using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;

public sealed record GetInvoiceByIdQuery(Guid InvoiceId) 
    : ICachedQuery<Result<InvoiceDto>>
{
    public string CacheKey => $"invoice_{InvoiceId}";

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);

    public string[] Tags => ["invoice"];
}