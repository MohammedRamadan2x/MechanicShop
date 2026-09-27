using MechanicShop.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using MediatR.Pipeline;

namespace MechanicShop.Application.Common.Behaviors;

public class LoggingBehavior<TRequest>(
    ILogger<TRequest> logger,
    IUser user,
    IIdentityService identityService)
    : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger = logger;
    private readonly IUser _user = user;
    private readonly IIdentityService _identityService = identityService;
    public async Task Process(TRequest request, CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _user.Id ?? string.Empty;
        string? userName = string.Empty;

        if (!string.IsNullOrEmpty(userId))
        {
            userName = await _identityService.GetUserNameAsync(userId);
        }

        _logger.LogInformation("Request: {Name} {@UserId} {@UserName} {@Request}", 
            requestName, userId, userName, request);
    }
}