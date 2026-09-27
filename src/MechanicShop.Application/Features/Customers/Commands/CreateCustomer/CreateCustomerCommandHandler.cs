using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler(
    ILogger<CreateCustomerCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache)
    : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    private readonly ILogger<CreateCustomerCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<CustomerDto>> Handle(
        CreateCustomerCommand command, 
        CancellationToken ct)
    {
        var email = command.Email.Trim().ToLower();

        var exists = await _context.Customers
            .AnyAsync(c => c.Email.ToLower() == email, ct);

        if (exists)
        {
            _logger.LogWarning(
                "Customer creation aborted. " +
                "Email already exists.");

            return CustomerErrors.EmailAlreadyExists;
        }

        List<Vehicle> vehicles = [];

        foreach (var v in command.Vehicles)
        {
            var createVehicleResult = Vehicle.Create(
                Guid.NewGuid(),
                v.Make,
                v.Model,
                v.Year,
                v.LicensePlate);

            if (createVehicleResult.IsFailure)
            {
                _logger.LogWarning(
                    "Customer creation failed while validating a vehicle. " +
                    "Errors: {@Errors}",
                    createVehicleResult.Errors);

                return createVehicleResult.Errors;
            }

            vehicles.Add(createVehicleResult.Value);
        }

        var createCustomerResult = Customer.Create(
            Guid.NewGuid(),
            command.Name.Trim(),
            command.PhoneNumber.Trim(),
            command.Email.Trim(),
            vehicles);

        if (createCustomerResult.IsFailure)
        {
            _logger.LogWarning(
                "Customer creation failed. " +
                "Errors: {@Errors}",
                createCustomerResult.Errors);

            return createCustomerResult.Errors;
        }

        _context.Customers.Add(createCustomerResult.Value);

        await _context.SaveChangesAsync(ct);

        await _cache.RemoveByTagAsync("customer", ct);

        var customer = createCustomerResult.Value;

        _logger.LogInformation(
            "Customer created successfully. " +
            "Id: {CustomerId}", 
            createCustomerResult.Value.Id);

        return customer.ToDto();
    }
}