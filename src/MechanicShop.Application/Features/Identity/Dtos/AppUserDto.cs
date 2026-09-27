using System.Security.Claims;

namespace MechanicShop.Application.Features.Identity.Dtos;

public sealed record AppUserDto(
    string UserId, 
    string Email, 
    IList<string> Roles, 
    IList<Claim> Claims);