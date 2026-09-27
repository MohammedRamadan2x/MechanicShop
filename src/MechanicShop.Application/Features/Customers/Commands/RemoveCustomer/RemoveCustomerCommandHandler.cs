using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;

public class RemoveCustomerCommandHandler(
    ILogger<RemoveCustomerCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache)
    : IRequestHandler<RemoveCustomerCommand, Result<Deleted>>
{
    private readonly ILogger<RemoveCustomerCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Deleted>> Handle(
        RemoveCustomerCommand command, 
        CancellationToken ct)
    {
        var customer = await _context.Customers
            .FindAsync([command.CustomerId], ct);

        if (customer is null)
        {
            _logger.LogWarning(
                "Customer with id {CustomerId} not found for deletion.",
                command.CustomerId);

            return ApplicationErrors.CustomerNotFound;
        }

        var hasAssociatedWorkOrders = await _context.WorkOrders
            .Include(wo => wo.Vehicle)
            .Where(wo => wo.Vehicle != null)
            .AnyAsync(wo => wo.Vehicle!.CustomerId == command.CustomerId, ct);

        if (hasAssociatedWorkOrders)
        {
            _logger.LogWarning(
                "Customer {CustomerId} cannot be deleted because they " +
                "have associated work orders (past, scheduled, or in-progress).", 
                command.CustomerId);

            return CustomerErrors.CannotDeleteCustomerWithWorkOrders;
        }

        _context.Customers.Remove(customer);

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("customer", ct);

        _logger.LogInformation(
            "Customer {CustomerId} deleted successfully.", 
            command.CustomerId);

        return Result.Deleted;
    }
}