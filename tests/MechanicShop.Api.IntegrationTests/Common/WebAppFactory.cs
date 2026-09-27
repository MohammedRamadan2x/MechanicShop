using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Infrastructure.BackgroundJobs;
using MechanicShop.Infrastructure.Data;

using MediatR;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

using Testcontainers.MsSql;

using Xunit;

namespace MechanicShop.Api.IntegrationTests.Common;

public class WebAppFactory : WebApplicationFactory<IAssemblyMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
    .Build();

    public AppHttpClient CreateAppHttpClient()
    {
        return new AppHttpClient(CreateClient());
    }

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
        });
    }
}