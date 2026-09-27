using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MechanicShop.Infrastructure.Identity;

public class IdentityService(
    UserManager<AppUser> userManager,
    IUserClaimsPrincipalFactory<AppUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService)
    : IIdentityService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly IUserClaimsPrincipalFactory<AppUser> _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    
    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user is not null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string? policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName!);

        return result.Succeeded;
    }

    public async Task<Result<AppUserDto>> AuthenticateAsync(
        string email, 
        string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return Error.NotFound(
                "User.NotFound", 
                $"User with email {UtilityService.MaskEmail(email)} not found");
        }

        if (!user.EmailConfirmed)
        {
            return Error.Conflict(
                "Email.NotConfirmed", 
                $"email '{UtilityService.MaskEmail(email)}' not confirmed");
        }

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            return Error.Conflict(
                "InvalidLoginAttempt", 
                "Email / Password are incorrect");
        }

        return new AppUserDto(
            user.Id, 
            user.Email!, 
            await _userManager.GetRolesAsync(user), 
            await _userManager.GetClaimsAsync(user));
    }

    public async Task<Result<AppUserDto>> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId) ?? 
            throw new InvalidOperationException(nameof(userId));

        var roles = await _userManager.GetRolesAsync(user);
        
        var claims = await _userManager.GetClaimsAsync(user);

        return new AppUserDto(user.Id, user.Email!, roles, claims); 
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }
}