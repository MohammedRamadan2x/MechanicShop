using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Identity.Queries.GenerateToken;

public sealed record GenerateTokenQuery(string Email, string Password) 
    : IRequest<Result<TokenResponse>>;