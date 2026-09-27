using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MechanicShop.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using MechanicShop.Domain.Common;

namespace MechanicShop.Infrastructure.Data;

public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IMediator mediator)
    : IdentityDbContext<AppUser>(options), IAppDbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<RepairTask> RepairTasks => Set<RepairTask>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var domainEntities = ChangeTracker.Entries()
            .Where(e => e.Entity is Entity baseEntity && 
                    baseEntity.DomainEvents.Count != 0)
            .Select(e => (Entity)e.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent, ct);
        }

        foreach (var entity in domainEntities)
        {
            entity.ClearDomainEvents();
        }
    }
}
