using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler(
    ILogger<UpdateCustomerCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache)
    : IRequestHandler<UpdateCustomerCommand, Result<Updated>>
{
    private readonly ILogger<UpdateCustomerCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<Updated>> Handle(
        UpdateCustomerCommand command, 
        CancellationToken ct)
    {
        var email = command.Email.Trim().ToLower();

        var exists = await _context.Customers
            .AnyAsync(c => 
                c.Id != command.CustomerId && c.Email.ToLower() == email, 
                ct);

        if (exists)
        {
            _logger.LogWarning(
                "Customer update aborted. " +
                "Customer {CustomerId} attempted to use an existing email {Email}.",
                command.CustomerId,
                command.Email);

            return CustomerErrors.EmailAlreadyExists;
        }

        var customer = await _context.Customers
             .Include(c => c.Vehicles)
             .FirstOrDefaultAsync(c => c.Id == command.CustomerId, ct);

        if (customer is null)
        {
            _logger.LogWarning(
                "Customer {CustomerId} not found for update.", 
                command.CustomerId);

            return ApplicationErrors.CustomerNotFound;
        }

        var validatedVehicles = new List<Vehicle>();

        foreach (var v in command.Vehicles)
        {
            var vehicleId = v.VehicleId ?? Guid.NewGuid();

            var vehicleResult = Vehicle.Create(
                vehicleId, 
                v.Make, 
                v.Model, 
                v.Year, 
                v.LicensePlate);

            if (vehicleResult.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to validate vehicle for Customer {CustomerId}. " +
                    "Errors: {@Errors}",
                    command.CustomerId,
                    vehicleResult.Errors);

                return vehicleResult.Errors;
            }

            validatedVehicles.Add(vehicleResult.Value);
        }

        var updateCustomerResult = customer.Update(
            command.Name.Trim(), 
            command.Email.Trim(), 
            command.PhoneNumber.Trim());

        if (updateCustomerResult.IsFailure)
        {
            _logger.LogWarning(
                "Failed to update Customer {CustomerId}. " +
                "Errors: {@Errors}",
                command.CustomerId,
                updateCustomerResult.Errors);

            return updateCustomerResult.Errors;
        }

        var upsertVehiclesResult = customer.UpsertVehicles(validatedVehicles);

        if (upsertVehiclesResult.IsFailure)
        {
            _logger.LogWarning(
                "Failed to update vehicles for Customer {CustomerId}. " +
                "Errors: {@Errors}",
                command.CustomerId,
                upsertVehiclesResult.Errors);

            return upsertVehiclesResult.Errors;
        }

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("customer", ct);

        _logger.LogInformation(
            "Customer {CustomerId} updated successfully.",
            customer.Id);

        return Result.Updated;
    }
}