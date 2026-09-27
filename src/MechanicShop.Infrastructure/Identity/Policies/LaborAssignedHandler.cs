using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace MechanicShop.Infrastructure.Identity.Policies;

public class LaborAssignedHandler(
    IAppDbContext dbContext,
    IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<LaborAssignedRequirement>
{
    private readonly IAppDbContext _dbContext = dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext authContext, 
        LaborAssignedRequirement requirement)
    {
        var userIdString = authContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString))
        {
            authContext.Fail();

            return;
        }

        if (!Guid.TryParse(userIdString, out var userId))
        {
            authContext.Fail();

            return;
        }

        var workOrderIdString = _httpContextAccessor
            .HttpContext?
            .Request
            .RouteValues["WorkOrderId"]?
            .ToString();

        if (!Guid.TryParse(workOrderIdString, out var workOrderId))
        {
            authContext.Fail();

            return;
        }

        var isAssigned = await _dbContext.WorkOrders
            .AnyAsync(wo => wo.Id == workOrderId && wo.LaborId == userId);

        if (isAssigned)
        {
            authContext.Succeed(requirement);

            return;
        }

        if (authContext.User.IsInRole(nameof(Role.Manager)))
        {
            authContext.Succeed(requirement);

            return;
        }

        authContext.Fail();
    }
}