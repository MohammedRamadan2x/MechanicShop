using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public sealed record CreateVehicleCommand(
    string Make,
    string Model,
    int Year,
    string LicensePlate) : IRequest<Result<VehicleDto>>;
