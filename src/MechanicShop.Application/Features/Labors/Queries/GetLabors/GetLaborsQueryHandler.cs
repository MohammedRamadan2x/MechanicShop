using MechanicShop.Application.Features.Labors.Mappers;
using MechanicShop.Application.Features.Labors.Dtos;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.Labors.Queries.GetLabors;

public class GetLaborsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetLaborsQuery, Result<List<LaborDto>>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<List<LaborDto>>> Handle(
        GetLaborsQuery query, 
        CancellationToken ct)
    {
        var labors = await _context.Employees
            .AsNoTracking()
            .Where(e => e.Role == Role.Labor)
            .ToListAsync(ct);

        return labors.ToDtos();
    }
}