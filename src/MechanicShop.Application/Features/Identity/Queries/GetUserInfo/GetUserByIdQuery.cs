using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserInfo;

public sealed record GetUserByIdQuery(string? UserId)
    : IRequest<Result<AppUserDto>>;