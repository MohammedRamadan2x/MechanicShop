using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Infrastructure.Data;

public class ApplicationDbContextInitialiser(
    ILogger<ApplicationDbContextInitialiser> logger,
    AppDbContext context, UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager)
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger = logger;
    private readonly AppDbContext _context = context;
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var managerRole = new IdentityRole(nameof(Role.Manager));

        if (_roleManager.Roles.All(r => r.Name != managerRole.Name))
        {
            await _roleManager.CreateAsync(managerRole);
        }

        var laborRole = new IdentityRole(nameof(Role.Labor));

        if (_roleManager.Roles.All(r => r.Name != laborRole.Name))
        {
            await _roleManager.CreateAsync(laborRole);
        }

        // Default users
        var manager = new AppUser
        {
            Id = "19a59129-6c20-417a-834d-11a208d32d96",
            Email = "pm@localhost",
            UserName = "pm@localhost",
            EmailConfirmed = true
        };

        if (_userManager.Users.All(u => u.Email != manager.Email))
        {
            await _userManager.CreateAsync(manager, manager.Email);

            if (!string.IsNullOrWhiteSpace(managerRole.Name))
            {
                await _userManager.AddToRolesAsync(manager, [managerRole.Name]);
            }
        }

        var labor01 = new AppUser
        {
            Id = "b6327240-0aea-46fc-863a-777fc4e42560",
            Email = "john.labor@localhost",
            UserName = "john.labor@localhost",
            EmailConfirmed = true
        };

        if (_userManager.Users.All(u => u.Email != labor01.Email))
        {
            await _userManager.CreateAsync(labor01, labor01.Email);

            if (!string.IsNullOrWhiteSpace(laborRole.Name))
            {
                await _userManager.AddToRolesAsync(labor01, [laborRole.Name]);
            }
        }

        var labor02 = new AppUser
        {
            Id = "8104ab20-26c2-4651-b1de-c0baf04dbbd9",
            Email = "peter.labor@localhost",
            UserName = "peter.labor@localhost",
            EmailConfirmed = true
        };

        if (_userManager.Users.All(u => u.Email != labor02.Email))
        {
            await _userManager.CreateAsync(labor02, labor02.Email);

            if (!string.IsNullOrWhiteSpace(laborRole.Name))
            {
                await _userManager.AddToRolesAsync(labor02, [laborRole.Name]);
            }
        }

        var labor03 = new AppUser
        {
            Id = "e17c83de-1089-4f19-bf79-5f789133d37f",
            Email = "kevin.labor@localhost",
            UserName = "kevin.labor@localhost",
            EmailConfirmed = true
        };

        if (_userManager.Users.All(u => u.Email != labor03.Email))
        {
            await _userManager.CreateAsync(labor03, labor03.Email);

            if (!string.IsNullOrWhiteSpace(laborRole.Name))
            {
                await _userManager.AddToRolesAsync(labor03, [laborRole.Name]);
            }
        }

        var labor04 = new AppUser
        {
            Id = "54cd01ba-b9ae-4c14-bab6-f3df0219ba4c",
            Email = "suzan.labor@localhost",
            UserName = "suzan.labor@localhost",
            EmailConfirmed = true
        };

        if (_userManager.Users.All(u => u.Email != labor04.Email))
        {
            await _userManager.CreateAsync(labor04, labor04.Email);

            if (!string.IsNullOrWhiteSpace(laborRole.Name))
            {
                await _userManager.AddToRolesAsync(labor04, [laborRole.Name]);
            }
        }

        if (!_context.Employees.Any())
        {
            _context.Employees.AddRange(
            [
                Employee.Create(Guid.Parse(manager.Id), "Primary", "Manager", Role.Manager).Value,
                Employee.Create(Guid.Parse(labor01.Id), "John", "S.", Role.Labor).Value,
                Employee.Create(Guid.Parse(labor02.Id), "Peter", "R.", Role.Labor).Value,
                Employee.Create(Guid.Parse(labor03.Id), "Kevin", "M.", Role.Labor).Value,
                Employee.Create(Guid.Parse(labor04.Id), "Suzan", "L.", Role.Labor).Value
            ]);
        }

        if (!_context.Customers.Any())
        {
            List<Vehicle> vehicles = [
                        Vehicle.Create(id: Guid.Parse("61401e63-007b-4b1c-8914-9eb6e9bd95c5"), make: "Toyota", model: "Camry", year: 2020, licensePlate: "ABC123").Value,
                        Vehicle.Create(id: Guid.Parse("13c80914-41ad-4d46-b7bb-60f6c89ad01e"), make: "Honda", model: "Civic", year: 2018, licensePlate: "XYZ456").Value,
                    ];

            _context.Customers.AddRange(
            [
                Customer.Create(id: Guid.Parse("f522bbe5-e3b1-4e2c-a8a3-c41550dcf39d"), name: "John Doe", phoneNumber: "123456789", email: "john.doe@localhost", vehicles: vehicles).Value,
                Customer.Create(id: Guid.Parse("73a04dd3-c81a-4a54-9882-ef1017eb192d"), name: "Sarah Peter", phoneNumber: "987654321", email: "sarah.peter@localhost", vehicles: [Vehicle.Create(id: Guid.Parse("a04f329d-0f5a-46a0-beae-699c034ae401"), make: "Ford", model: "Focus", year: 2021, licensePlate: "DEF789").Value, Vehicle.Create(id: Guid.Parse("cf60e95b-5752-4c26-aa07-31a34164606c"), make: "Chevrolet", model: "Malibu", year: 2019, licensePlate: "GHI012").Value,]).Value,
            ]);
        }

        if (!_context.RepairTasks.Any())
        {
            _context.RepairTasks.AddRange([
                RepairTask.Create(id: Guid.Parse("616aebb1-d515-4b40-8d47-8d5c0b67a313"), name: "Engine Oil Change", laborCost: 50.00m, estimatedDurationInMins: RepairDurationInMinutes.Min60, parts: [Part.Create(Guid.Parse("ec65225c-9066-4a1c-974f-f183c39fdd16"), "Engine Oil", 25.00m, 1).Value, Part.Create(Guid.Parse("62ad80e3-2cff-41af-ab40-16fab8db8b38"), "Oil Filter", 10.00m, 1).Value]).Value,
                RepairTask.Create(id: Guid.Parse("4fa0be55-06f6-4616-b086-e1f0c9354cd8"), name: "Brake Replacement", laborCost: 150.00m, estimatedDurationInMins: RepairDurationInMinutes.Min90, parts: [Part.Create(Guid.Parse("86375a12-715e-4aa4-aad9-c0f9ccf44a14"), "Brake Pads", 40.00m, 2).Value, Part.Create(Guid.Parse("526d89c3-a971-4ea7-ba15-de6b50b13c21"), "Brake Fluid", 15.00m, 1).Value]).Value,
                RepairTask.Create(id: Guid.Parse("a376b5d1-6b2d-4dd8-883e-d3d1721c1316"), name: "Tire Rotation", laborCost: 30.00m, estimatedDurationInMins: RepairDurationInMinutes.Min45, parts: [Part.Create(Guid.Parse("a46f974e-a198-4098-8a1f-6be6e68ec743"), "Tire Valve", 5.00m, 4).Value]).Value,
                RepairTask.Create(id: Guid.Parse("a770cc6e-0c8b-4ac5-9ee6-6928682bd47e"), name: "Battery Replacement", laborCost: 70.00m, estimatedDurationInMins: RepairDurationInMinutes.Min30, parts: [Part.Create(Guid.Parse("d4fd3255-29dc-4d45-9d87-f58ab98bc28b"), "Car Battery", 120.00m, 1).Value]).Value,
                RepairTask.Create(id: Guid.Parse("e4c2b675-4a60-488f-a7b4-61966e7e80e3"), name: "Wheel Alignment", laborCost: 80.00m, estimatedDurationInMins: RepairDurationInMinutes.Min60, parts: [Part.Create(Guid.Parse("fa3b9a7e-1c2d-4e3f-9b8a-0c1d2e3f4a5b"), "Alignment Shim Kit (per wheel)", 5.00m, 4).Value]).Value,
                RepairTask.Create(id: Guid.Parse("1cb1608c-3bc7-4325-99c3-8244c0fb412f"), name: "Air Conditioning Recharge", laborCost: 100.00m, estimatedDurationInMins: RepairDurationInMinutes.Min30, parts: [Part.Create(Guid.Parse("526dca0a-d236-47d3-8e8f-c83d555b2de9"), "Refrigerant", 50.00m, 1).Value]).Value,
                RepairTask.Create(id: Guid.Parse("a8e9b4e0-8581-40df-967d-51a0f4fabc0e"), name: "Spark Plug Replacement", laborCost: 90.00m, estimatedDurationInMins: RepairDurationInMinutes.Min60, parts: [Part.Create(Guid.Parse("019f5eab-a8a5-44d4-92b3-1f998e3f10c2"), "Spark Plug", 10.00m, 4).Value]).Value,
                RepairTask.Create(id: Guid.Parse("90f2f3ef-3357-439e-9689-628aa08200c1"), name: "Engine Diagnostic", laborCost: 120.00m, estimatedDurationInMins: RepairDurationInMinutes.Min120, parts: [Part.Create(Guid.Parse("c3d4e5f6-a7b8-9c0d-1e2f-3a4b5c6d7e8f"), "Smoke Leak Detector Fluid Cartridge", 20.00m, 1).Value]).Value,
                RepairTask.Create(id: Guid.Parse("d124651e-ca72-467e-ba28-81ea4a2080bc"), name: "Timing Belt Replacement", laborCost: 200.00m, estimatedDurationInMins: RepairDurationInMinutes.Min120, parts: [Part.Create(Guid.Parse("06b764a0-73a2-4c37-b279-adae3856499c"), "Timing Belt", 75.00m, 1).Value]).Value,
                RepairTask.Create(id: Guid.Parse("cee9b309-8620-4028-8d38-2532771ab3ea"), name: "Transmission Fluid Change", laborCost: 100.00m, estimatedDurationInMins: RepairDurationInMinutes.Min45, parts: [Part.Create(Guid.Parse("0a8b0c19-873a-4da0-811b-45ff85bca0ed"), "Transmission Fluid", 60.00m, 1).Value]).Value
            ]);
        }

        await _context.SaveChangesAsync();

        if (!_context.WorkOrders.Any())
        {
            var repairTasks = _context.RepairTasks.ToList();
            var vehicles = _context.Vehicles.ToList();
            string[] labors = [labor01.Id, labor02.Id, labor03.Id, labor04.Id];
            Spot[] spots = [Spot.A, Spot.B, Spot.C, Spot.D];

            var generatedWorkOrders = new List<WorkOrder>();
            Random random = new();
            DateTimeOffset startDate = DateTimeOffset.Now.Date.AddDays(1); // Start from tomorrow
            DateTimeOffset endDate = startDate.AddMonths(1); // Generate for the next month
            TimeSpan openTime = TimeSpan.FromHours(12);  // 9:00 am utc -4
            TimeSpan closeTime = TimeSpan.FromHours(23); // 18:00 am utc -4
            int totalMinutes = (int)(closeTime - openTime).TotalMinutes;

            while (startDate < endDate)
            {
                foreach (Spot spot in spots)
                {
                    int occupiedMinutes = 0;
                    int minOccupancy = (int)(totalMinutes * 0.6); // Minimum 60% usage
                    int maxOccupancy = (int)(totalMinutes * 0.8); // Maximum 80% usage
                    List<WorkOrder> spotWorkOrders = [];

                    DateTimeOffset currentTime = startDate.Add(openTime);

                    while (occupiedMinutes < minOccupancy && currentTime.TimeOfDay < closeTime)
                    {
                        var selectedTask = repairTasks
                                            .DistinctBy(t => t.Id)
                                            .OrderBy(_ => Guid.NewGuid())
                                            .Take(Random.Shared.Next(1, Math.Min(4, repairTasks.Select(t => t.Id).Distinct().Count())))
                                            .ToList();
                        var laborId = labors[random.Next(labors.Length)];
                        var duration = selectedTask.Sum(st => (int)st.EstimatedDurationInMins);

                        if (occupiedMinutes + duration > maxOccupancy)
                        {
                            break;
                        }

                        DateTimeOffset startAt = currentTime;
                        DateTimeOffset endAt = startAt.AddMinutes(duration);

                        var availableVehicle = vehicles
                        .Where(v => !generatedWorkOrders.Any(w =>
                            w.VehicleId == v.Id &&
                            w.StartAtUtc.Date == startAt.Date &&
                            w.StartAtUtc < endAt &&
                            w.EndAtUtc > startAt))
                        .OrderBy(_ => Guid.NewGuid())
                        .FirstOrDefault();

                        if (availableVehicle == null)
                        {
                            break;
                        }

                        if (endAt.TimeOfDay > closeTime)
                        {
                            break;
                        }

                        var workOrder = WorkOrder.Create(
                            Guid.NewGuid(),
                            availableVehicle.Id,
                            startAt,
                            endAt,
                            Guid.Parse(laborId),
                            spot,
                            selectedTask);

                        spotWorkOrders.Add(workOrder.Value);
                        occupiedMinutes += duration;

                        currentTime = startDate.Add(openTime).AddMinutes(occupiedMinutes);
                    }

                    // Ensure at least 60% occupancy is reached for this spot before adding WorkOrders
                    if (occupiedMinutes >= minOccupancy)
                    {
                        generatedWorkOrders.AddRange(spotWorkOrders);
                    }
                }

                startDate = startDate.AddDays(1);
            }

            var repairTasksForFirstOrder = _context.RepairTasks
             .OrderBy(_ => Guid.NewGuid())
             .Take(2)
             .ToList();

            var utcNow = DateTimeOffset.UtcNow;

            // Round down to nearest 30-minute block
            var floored = new DateTimeOffset(
                  utcNow.Year,
                  utcNow.Month,
                  utcNow.Day,
                  utcNow.Hour,
                  utcNow.Minute - (utcNow.Minute % 15),
                  0,
                  TimeSpan.Zero); // keep in UTC

            var startTimeFirstOrder = floored;

            var workOrderStartingNow = WorkOrder.Create(
                Guid.NewGuid(),
                _context.Vehicles.OrderBy(_ => Guid.NewGuid()).First().Id,
                startTimeFirstOrder,
                startTimeFirstOrder.AddMinutes(repairTasksForFirstOrder.Sum(rt => (int)rt.EstimatedDurationInMins)),
                Guid.Parse(labor01.Id),
                Spot.A,
                repairTasksForFirstOrder).Value;

            workOrderStartingNow.UpdateState(WorkOrderState.InProgress);

            var repairTasksEndingNow = _context.RepairTasks
      .First(rt => rt.EstimatedDurationInMins == RepairDurationInMinutes.Min60);

            // Align to 15-minute slot: started 45 minutes ago
            var startedAgo = utcNow.AddMinutes(-45);
            var roundedStart = new DateTimeOffset(
                startedAgo.Year,
                startedAgo.Month,
                startedAgo.Day,
                startedAgo.Hour,
                startedAgo.Minute - (startedAgo.Minute % 15),
                0,
                TimeSpan.Zero);

            var endTimeSecondOrder = roundedStart.AddMinutes((int)repairTasksEndingNow.EstimatedDurationInMins);

            WorkOrder value = WorkOrder.Create(
                Guid.NewGuid(),
                _context.Vehicles.OrderBy(_ => Guid.NewGuid()).First().Id,
                roundedStart,
                endTimeSecondOrder,
                Guid.Parse(labor02.Id),
                Spot.B,
                [repairTasksEndingNow])
            .Value;
            var workOrderEndingNow = value;

            workOrderEndingNow.UpdateState(WorkOrderState.InProgress);

            generatedWorkOrders.AddRange(workOrderStartingNow, workOrderEndingNow);

            _context.WorkOrders.AddRange(generatedWorkOrders);

            await _context.SaveChangesAsync();
        }
    }
}

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}