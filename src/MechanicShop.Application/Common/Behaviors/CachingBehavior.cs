using MechanicShop.Domain.Common.Results.Abstractions;
using MechanicShop.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using MediatR;

namespace MechanicShop.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse>(
    HybridCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly HybridCache _cache = cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (request is not ICachedQuery cachedRequest)
        {
            return await next(ct);
        }

        _logger.LogInformation("Checking cache for {RequestName}", 
            typeof(TRequest).Name);

        var result = await _cache.GetOrCreateAsync<TResponse>(
            cachedRequest.CacheKey,
            _ => new ValueTask<TResponse>((TResponse)(object)null!),
            new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.DisableUnderlyingData
            },
            cancellationToken: ct);

        if (result is null)
        {
            result = await next(ct);

            if (result is IResult res && res.IsSuccess)
            {
                _logger.LogInformation("Caching result for {RequestName}", 
                    typeof(TRequest).Name);

                await _cache.SetAsync(
                    cachedRequest.CacheKey,
                    result,
                    new HybridCacheEntryOptions
                    {
                        Expiration = cachedRequest.Expiration
                    },
                    cachedRequest.Tags,
                    ct);
            }
        }

        return result;
    }
}