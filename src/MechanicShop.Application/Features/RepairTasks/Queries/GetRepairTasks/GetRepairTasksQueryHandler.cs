using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;

public class GetRepairTasksQueryHandler(IAppDbContext context)
    : IRequestHandler<GetRepairTasksQuery, Result<List<RepairTaskDto>>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<List<RepairTaskDto>>> Handle(
        GetRepairTasksQuery query, 
        CancellationToken ct)
    {
        var repairTasks = await _context.RepairTasks
            .AsNoTracking()
            .Include(rt => rt.Parts)
            .ToListAsync(ct);

        return repairTasks.ToDtos();
    }
}