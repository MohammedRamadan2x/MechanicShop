using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Identity.Queries.RefreshToken;

public sealed record RefreshTokenQuery(
    string RefreshToken, 
    string ExpiredAccessToken) 
    : IRequest<Result<TokenResponse>>;