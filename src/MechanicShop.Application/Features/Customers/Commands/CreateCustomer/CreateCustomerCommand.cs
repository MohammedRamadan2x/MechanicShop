using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(
    string Name,
    string PhoneNumber,
    string Email,
    List<CreateVehicleCommand> Vehicles) : IRequest<Result<CustomerDto>>;