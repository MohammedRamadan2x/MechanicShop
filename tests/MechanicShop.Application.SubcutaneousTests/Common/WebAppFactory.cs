using MechanicShop.Infrastructure.BackgroundJobs;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Infrastructure.Settings;
using MechanicShop.Infrastructure.Data;
using MechanicShop.Api;

using MediatR;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using Testcontainers.MsSql;

namespace MechanicShop.Application.SubcutaneousTests.Common;


public class WebAppFactory : WebApplicationFactory<IAssemblyMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder().Build();

    public IMediator CreateMediator()
    {
        var serviceScope = Services.CreateScope();

        return serviceScope.ServiceProvider.GetRequiredService<IMediator>();
    }

    public IAppDbContext CreateAppDbContext()
    {
        var serviceScope = Services.CreateScope();

        return serviceScope.ServiceProvider.GetRequiredService<IAppDbContext>();
    }

    public Task InitializeAsync()
    {
        return _dbContainer.StartAsync()
          .ContinueWith(async _ =>
          {
              using var scope = Services.CreateScope();
              var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

              context.WorkOrders.RemoveRange(context.WorkOrders);
              await context.SaveChangesAsync();
          }).Unwrap();
    }

    public new Task DisposeAsync() => _dbContainer.StopAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<OverdueBookingCleanupService>();

            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseSqlServer(_dbContainer.GetConnectionString());
            });

            services.RemoveAll<AppSettings>();

            // Explicit override AFTER Configure
            services.PostConfigure<AppSettings>(opts =>
            {
                opts.OpeningTime = new TimeOnly(9, 0);
                opts.ClosingTime = new TimeOnly(18, 0);
            });
        });
    }
}