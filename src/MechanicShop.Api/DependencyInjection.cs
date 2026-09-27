using Asp.Versioning;

using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Api.OpenApi.Transformers;
using MechanicShop.Infrastructure.Settings;
using MechanicShop.Api.Infrastructure;
using MechanicShop.Api.Services;

using Microsoft.AspNetCore.RateLimiting;

using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

using Serilog;

using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

namespace MechanicShop.Api;


public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        services.AddCustomProblemDetails()
                .AddCustomApiVersioning()
                .AddApiDocumentation()
                .AddExceptionHandling()
                .AddControllerWithJsonConfiguration()
                .AddValidation()
                .AddConfiguredCors(configuration)
                .AddIdentityInfrastructure()
                .AddAppRateLimiting()
                .AddAppOutputCaching()
                .AddAppOpenTelememrty()
                .AddSignalR();

        return services;
    }

    public static IServiceCollection AddAppOutputCaching(
    this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            options.SizeLimit = 100 * 1024 * 1024; 
        });

        return services;
    }

    public static IServiceCollection AddAppOpenTelememrty(
        this IServiceCollection services)
    {
        services.AddOpenTelemetry()
        .ConfigureResource(res => res.AddService("orderservice"))
        .WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation().
            AddHttpClientInstrumentation();

            tracing.AddOtlpExporter();
        }).
        WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation().
            AddHttpClientInstrumentation();

            metrics.AddOtlpExporter().
            AddPrometheusExporter(); // /metrics
        });

        return services;
    }

    public static IServiceCollection AddAppRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddSlidingWindowLimiter("SlidingWindow", limiterOptions =>
            {
                limiterOptions.PermitLimit = 100;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.SegmentsPerWindow = 6;
                limiterOptions.QueueLimit = 10;
                limiterOptions.QueueProcessingOrder = 
                    QueueProcessingOrder.OldestFirst;
                limiterOptions.AutoReplenishment = true;
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }

    public static IServiceCollection AddCustomProblemDetails(
        this IServiceCollection services)
    {
        services.AddProblemDetails(options => 
            options.CustomizeProblemDetails = (context) =>
            {
                context.ProblemDetails.Instance = 
                    $"{context.HttpContext.Request.Method} " +
                    $"{context.HttpContext.Request.Path}";

                context.ProblemDetails.Extensions
                .Add("requestId", context.HttpContext.TraceIdentifier);
            });

        return services;
    }

    public static IServiceCollection AddCustomApiVersioning(
        this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    public static IServiceCollection AddApiDocumentation(
        this IServiceCollection services)
    {
        string[] versions = ["v1"];

        foreach (var version in versions)
        {
            services.AddOpenApi(version, options =>
            {
                // Versioning config
                options.AddDocumentTransformer<VersionInfoTransformer>();

                // Security Scheme config
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                options.AddOperationTransformer<BearerSecuritySchemeTransformer>();
            });
        }

        return services;
    }

    public static IServiceCollection AddExceptionHandling(
        this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    public static IServiceCollection AddControllerWithJsonConfiguration(
        this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options => options
            .JsonSerializerOptions
            .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

        return services;
    }

    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IUser, CurrentUser>();

        services.AddHttpContextAccessor();

        return services;
    }

    public static IServiceCollection AddConfiguredCors(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>()!;

        services.AddCors(options => options.AddPolicy(
            appSettings.CorsPolicyName,
            policy => policy
                .WithOrigins(appSettings.AllowedOrigins!)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));

        return services;
    }

    public static IApplicationBuilder UseCoreMiddlewares(
        this IApplicationBuilder app, 
        IConfiguration configuration)
    {
        // 1. Exception handling should be FIRST to catch all errors
        app.UseExceptionHandler();

        // 2. Status code pages for handling HTTP status codes
        app.UseStatusCodePages();

        // 3. HTTPS redirection (before any other middleware that might generate URLs)
        app.UseHttpsRedirection();

        // 4. Serilog request logging (early to log all requests)
        app.UseSerilogRequestLogging();

        // 5. CORS (before authentication/authorization)
        app.UseCors(configuration["AppSettings:CorsPolicyName"]!);

        // 6. Rate limiting (before authentication to protect auth endpoints)
        app.UseRateLimiter();

        // 7. Authentication (must come before authorization)
        app.UseAuthentication();

        // 8. Authorization (must come after authentication)
        app.UseAuthorization();

        // 9. Output caching (after auth to cache based on user context)
        app.UseOutputCache();

        return app;
    }
}