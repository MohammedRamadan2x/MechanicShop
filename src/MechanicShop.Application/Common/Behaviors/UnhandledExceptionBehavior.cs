using Microsoft.Extensions.Logging;
using MediatR;

namespace MechanicShop.Application.Common.Behaviors;

public class UnhandledExceptionBehaviour<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken ct)
    {
        try
        {
            return await next(ct);
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogError(ex, 
                "Request: Unhandled Exception for Request {Name} {@Request}", 
                requestName, request);

            throw;
        }
    }
}