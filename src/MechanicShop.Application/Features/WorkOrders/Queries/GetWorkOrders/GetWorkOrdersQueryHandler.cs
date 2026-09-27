using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;


public class GetWorkOrdersQueryHandler(IAppDbContext context)
    : IRequestHandler<GetWorkOrdersQuery, Result<PaginatedList<WorkOrderListItemDto>>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<PaginatedList<WorkOrderListItemDto>>> Handle(
        GetWorkOrdersQuery query, 
        CancellationToken ct)
    {
        var workOrdersQuery = _context.WorkOrders
            .AsNoTracking()
            .Include(wo => wo.Vehicle!)
                .ThenInclude(v => v.Customer)
            .Include(wo => wo.Labor)
            .Include(wo => wo.RepairTasks)
                .ThenInclude(rt => rt.Parts)
            .Include(wo => wo.Invoice)
            .AsQueryable();

        workOrdersQuery = ApplyFilters(workOrdersQuery, query);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            workOrdersQuery = ApplySearchTerm(workOrdersQuery, query.SearchTerm);
        }

        workOrdersQuery = ApplySorting(
            workOrdersQuery, 
            query.SortColumn, 
            query.SortDirection);

        var count = await workOrdersQuery.CountAsync(ct);

        var items = await workOrdersQuery
              .Skip((query.Page - 1) * query.PageSize)
              .Take(query.PageSize)
              .Select(wo => new WorkOrderListItemDto
              {
                  WorkOrderId = wo.Id,
                  InvoiceId = wo.Invoice == null ? null : wo.Invoice.Id,
                  Spot = wo.Spot,
                  StartAtUtc = wo.StartAtUtc,
                  EndAtUtc = wo.EndAtUtc,
                  Vehicle = wo.Vehicle!.ToDto(),
                  Customer = wo.Vehicle!.Customer!.Name,
                  Labor = wo.Labor != null
                    ? wo.Labor.FirstName + " " + wo.Labor.LastName
                    : null,
                  State = wo.State,
                  RepairTasks = wo.RepairTasks.Select(rt => rt.Name).ToList()
              })
              .ToListAsync(ct);

        return new PaginatedList<WorkOrderListItemDto>
        {
            Items = items,
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalCount = count,
            TotalPages = (int)Math.Ceiling(count / (double)query.PageSize)
        };
    }

    private static IQueryable<WorkOrder> ApplyFilters(
        IQueryable<WorkOrder> query, 
        GetWorkOrdersQuery searchQuery)
    {
        if (searchQuery.State.HasValue)
        {
            query = query.Where(wo => wo.State == searchQuery.State.Value);
        }

        if (searchQuery.VehicleId.HasValue && searchQuery.VehicleId != Guid.Empty)
        {
            query = query.Where(wo => wo.VehicleId == searchQuery.VehicleId.Value);
        }

        if (searchQuery.LaborId.HasValue && searchQuery.LaborId != Guid.Empty)
        {
            query = query.Where(wo => wo.LaborId == searchQuery.LaborId.Value);
        }

        if (searchQuery.StartDateFrom.HasValue)
        {
            query = query.Where(wo => wo.StartAtUtc >= searchQuery.StartDateFrom.Value);
        }

        if (searchQuery.StartDateTo.HasValue)
        {
            query = query.Where(wo => wo.StartAtUtc <= searchQuery.StartDateTo.Value);
        }

        if (searchQuery.EndDateFrom.HasValue)
        {
            query = query.Where(wo => wo.EndAtUtc >= searchQuery.EndDateFrom.Value);
        }

        if (searchQuery.EndDateTo.HasValue)
        {
            query = query.Where(wo => wo.EndAtUtc <= searchQuery.EndDateTo.Value);
        }

        if (searchQuery.Spot.HasValue)
        {
            query = query.Where(wo => wo.Spot == searchQuery.Spot.Value);
        }

        return query;
    }

    private static IQueryable<WorkOrder> ApplySearchTerm(
        IQueryable<WorkOrder> query, 
        string searchTerm)
    {
        var normalized = searchTerm.Trim().ToLower();

        return query.Where(wo =>
            (wo.Vehicle != null && (
                wo.Vehicle.Make.ToLower().Contains(normalized) ||
                wo.Vehicle.Model.ToLower().Contains(normalized) ||
                wo.Vehicle.LicensePlate.ToLower().Contains(normalized)
            )) ||
            (wo.Labor != null && (
                wo.Labor.FirstName.ToLower().Contains(normalized) ||
                wo.Labor.LastName.ToLower().Contains(normalized) ||
                (wo.Labor.FirstName + " " + wo.Labor.LastName).ToLower()
                .Contains(normalized)
            )) ||
            wo.RepairTasks.Any(rt =>
                rt.Name.ToLower().Contains(normalized)) ||
            wo.Id.ToString().ToLower().Contains(normalized));
    }

    private static IQueryable<WorkOrder> ApplySorting(
        IQueryable<WorkOrder> query, 
        string sortColumn, 
        string sortDirection)
    {
        var isDescending = sortDirection
            .Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortColumn.ToLower() switch
        {
            "createdat" => isDescending ? 
            query.OrderByDescending(wo => wo.CreatedAtUtc) 
            : query.OrderBy(wo => wo.CreatedAtUtc),
            
            "updatedat" => isDescending ? 
            query.OrderByDescending(wo => wo.LastModifiedAtUtc) 
            : query.OrderBy(wo => wo.LastModifiedAtUtc),
            
            "startat" => isDescending ? 
            query.OrderByDescending(wo => wo.StartAtUtc) 
            : query.OrderBy(wo => wo.StartAtUtc),
            
            "endat" => isDescending ? 
            query.OrderByDescending(wo => wo.EndAtUtc) 
            : query.OrderBy(wo => wo.EndAtUtc),
            
            "state" => isDescending ? 
            query.OrderByDescending(wo => wo.State) 
            : query.OrderBy(wo => wo.State),
            
            "spot" => isDescending ? 
            query.OrderByDescending(wo => wo.Spot) 
            : query.OrderBy(wo => wo.Spot),
            
            "total" => isDescending ? 
            query.OrderByDescending(wo => wo.Total) 
            : query.OrderBy(wo => wo.Total),
            
            "vehicleid" => isDescending ? 
            query.OrderByDescending(wo => wo.VehicleId) 
            : query.OrderBy(wo => wo.VehicleId),
            
            "laborid" => isDescending ? 
            query.OrderByDescending(wo => wo.LaborId) 
            : query.OrderBy(wo => wo.LaborId),
            
            _ => query.OrderByDescending(wo => wo.CreatedAtUtc) // Default sorting
        };
    }
}