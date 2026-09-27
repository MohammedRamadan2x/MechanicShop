using MechanicShop.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR.Pipeline;

namespace MechanicShop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddTransient(
            typeof(IRequestPreProcessor<>),
            typeof(LoggingBehavior<>));

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
        });

        return services;
    }
}
