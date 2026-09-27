using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Contracts.Requests.WorkOrders;
using MechanicShop.Api.IntegrationTests.Common;
using MechanicShop.Application.Common.Models;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Security;

using System.Net;

using Xunit;

using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Api.IntegrationTests.Controllers;

[Collection(WebAppFactoryCollection.CollectionName)]
public class WorkOrdersControllerTests(WebAppFactory webAppFactory)
{
    private readonly AppHttpClient _client = webAppFactory.CreateAppHttpClient();
    private readonly IAppDbContext _context = webAppFactory.CreateAppDbContext();

    [Fact]
    public async Task GetWorkOrders_WithValidPagination_ShouldReturnPaginatedList()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync("/api/v1.0/workorders?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PaginatedList<WorkOrderListItemDto>>();
        Assert.NotNull(result);
        Assert.NotNull(result!.Items);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
    }

    [Theory]
    [InlineData(0, 10, "Page must be greater than 0")]
    [InlineData(-1, 10, "Page must be greater than 0")]
    [InlineData(1, 0, "PageSize must be between 1 and 100")]
    [InlineData(1, 101, "PageSize must be between 1 and 100")]
    [InlineData(1, -1, "PageSize must be between 1 and 100")]
    public async Task GetWorkOrders_WithInvalidPagination_ShouldReturnBadRequest(int page, int pageSize, string expectedError)
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var response = await _client.GetAsync($"/api/v1.0/workorders?page={page}&pageSize={pageSize}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains(expectedError, content);
    }

    [Fact]
    public async Task GetWorkOrders_WithFilters_ShouldApplyFiltersCorrectly()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var vehicleId = Guid.NewGuid();
        var laborId = Guid.NewGuid();
        const string searchTerm = "test";
        const int state = (int)WorkOrderState.InProgress;
        const int spot = (int)Contracts.Common.Spot.A;
        var startDateFrom = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var startDateTo = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var queryString = $"page=1&pageSize=10&searchTerm={searchTerm}&state={state}&vehicleId={vehicleId}&laborId={laborId}&spot={spot}&startDateFrom={startDateFrom}&startDateTo={startDateTo}";

        var response = await _client.GetAsync($"/api/v1.0/workorders?{queryString}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PaginatedList<WorkOrderListItemDto>>();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetWorkOrders_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1.0/workorders?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkOrderById_WithValidId_ShouldReturnWorkOrder()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
                   .ForToday()
                   .WithRepairTasks(await _context.RepairTasks.Take(1).ToListAsync())
                   .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
                   .WithLabor(TestUsers.Labor01.Id)
                   .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(default);

        try
        {
            var response = await _client.GetAsync($"/api/v1.0/workorders/{workOrder.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<WorkOrderDto>();

            Assert.NotNull(result);
            Assert.Equal(workOrder.Id, result!.WorkOrderId);
        }
        finally
        {
            await _context.WorkOrders
                .Where(w => w.Id == workOrder.Id)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task GetWorkOrderById_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/v1.0/workorders/{nonExistentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkOrderById_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var workOrderId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/v1.0/workorders/{workOrderId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkOrder_WithValidRequest_ShouldCreateWorkOrder()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var laborId = Guid.Parse(TestUsers.Labor01.Id);
        var customer = (await _context.Customers.Include(c => c.Vehicles).FirstOrDefaultAsync())!;
        var vehicle = customer.Vehicles.FirstOrDefault()!;
        var repairTaskIds = _context.RepairTasks.Select(rt => rt.Id).Take(2).ToList()!;

        var request = new CreateWorkOrderRequest
        {
            Spot = Contracts.Common.Spot.B,
            VehicleId = vehicle.Id,
            StartAtUtc = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(12),
            LaborId = laborId,
            RepairTaskIds = repairTaskIds
        };

        WorkOrderDto? dto = null;
        try
        {
            var response = await _client.PostAsJsonAsync("/api/v1.0/workorders", request);

            dto = await response.Content.ReadFromJsonAsync<WorkOrderDto>();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            Assert.NotNull(dto);
        }
        finally
        {
            if (dto is not null)
            {
                await _context.WorkOrders
                  .Where(w => w.Id == dto.WorkOrderId)
                  .ExecuteDeleteAsync();
            }
        }
    }

    [Fact]
    public async Task CreateWorkOrder_WithInvalidRequest_ShouldReturnBadRequest()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var request = new CreateWorkOrderRequest
        {
            VehicleId = Guid.Empty,
            StartAtUtc = default,
            RepairTaskIds = []
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/workorders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkOrder_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor02);

        _client.SetAuthorizationHeader(token);

        var request = new CreateWorkOrderRequest
        {
            Spot = Contracts.Common.Spot.A,
            VehicleId = Guid.NewGuid(),
            StartAtUtc = DateTime.UtcNow.AddHours(1),
            RepairTaskIds = [Guid.NewGuid()],
            LaborId = Guid.NewGuid()
        };

        var response = await _client.PostAsJsonAsync("/api/v1.0/workorders", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RelocateWorkOrder_WithValidRequest_ShouldUpdateWorkOrder()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
               .ForToday()
               .WithRepairTasks(await _context.RepairTasks.Take(1).ToListAsync())
               .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
               .WithLabor(TestUsers.Labor01.Id)
               .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(default);

        var request = new RelocateWorkOrderRequest
        {
            NewStartAtUtc = DateTime.UtcNow.AddHours(2),
            NewSpot = Contracts.Common.Spot.B
        };

        try
        {
            var response = await _client.PutAsJsonAsync($"/api/v1.0/workorders/{workOrder.Id}/relocation", request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            await _context.WorkOrders
                 .Where(w => w.Id == workOrder.Id)
                 .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task RelocateWorkOrder_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        var request = new RelocateWorkOrderRequest
        {
            NewStartAtUtc = DateTime.UtcNow.AddHours(2),
            NewSpot = Contracts.Common.Spot.B
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/workorders/{nonExistentId}/relocation", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AssignLabor_WithValidRequest_ShouldAssignLabor()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
                       .ForToday()
                       .WithRepairTasks(await _context.RepairTasks.Take(1).ToListAsync())
                       .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
                       .WithLabor(TestUsers.Labor01.Id)
                       .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(default);

        var request = new AssignLaborRequest
        {
            LaborId = Guid.Parse(TestUsers.Labor02.Id)
        };

        try
        {
            var response = await _client.PutAsJsonAsync($"/api/v1.0/workorders/{workOrder.Id}/labor", request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            await _context.WorkOrders
                 .Where(w => w.Id == workOrder.Id)
                 .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task AssignLabor_WithInvalidLaborId_ShouldReturnBadRequest()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var request = new AssignLaborRequest
        {
            LaborId = Guid.Empty
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/workorders/{Guid.NewGuid()}/labor", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWorkOrderState_WithValidRequest_ShouldUpdateState()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
                    .ForToday()
                    .WithRepairTasks(await _context.RepairTasks.Take(1).ToListAsync())
                    .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
                    .WithTimeSlot(DateTimeOffset.UtcNow.AddMinutes(-3), DateTimeOffset.UtcNow.AddMinutes(30))
                    .WithLabor(TestUsers.Labor01.Id)
                    .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(default);

        try
        {
            var request = new UpdateWorkOrderStateRequest
            {
                State = Contracts.Common.WorkOrderState.InProgress
            };

            var response = await _client.PutAsJsonAsync($"/api/v1.0/workorders/{workOrder.Id}/state", request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            await _context.WorkOrders
                 .Where(w => w.Id == workOrder.Id)
                 .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task UpdateWorkOrderState_AsLabor_WithSelfScopedAccess_ShouldSucceed()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
          .ForToday()
          .WithRepairTasks(await _context.RepairTasks.Take(1).ToListAsync())
          .WithTimeSlot(DateTimeOffset.UtcNow.AddMinutes(-3), DateTimeOffset.UtcNow.AddMinutes(30))
          .WithLabor(TestUsers.Labor01.Id)
          .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
          .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(default);

        try
        {
            var request = new UpdateWorkOrderStateRequest
            {
                State = Contracts.Common.WorkOrderState.InProgress
            };

            var response = await _client.PutAsJsonAsync($"/api/v1.0/workorders/{workOrder.Id}/state", request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            await _context.WorkOrders
                .Where(w => w.Id == workOrder.Id)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task UpdateWorkOrderState_AsLabor_WithoutSelfScopedAccess_ShouldSucceed()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor02);

        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
         .ForToday()
         .WithRepairTasks(await _context.RepairTasks.Take(1).ToListAsync())
         .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
         .WithLabor(TestUsers.Labor01.Id)
         .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(default);

        try
        {
            var request = new UpdateWorkOrderStateRequest
            {
                State = Contracts.Common.WorkOrderState.InProgress
            };

            var response = await _client.PutAsJsonAsync($"/api/v1.0/workorders/{workOrder.Id}/state", request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            await _context.WorkOrders
                 .Where(w => w.Id == workOrder.Id)
                 .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task UpdateRepairTasks_WithValidRequest_ShouldUpdateTasks()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
          .ForToday()
          .WithRepairTasks(await _context.RepairTasks.Take(1).ToListAsync())
          .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
          .WithLabor(Guid.Parse(TestUsers.Labor01.Id))
          .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(default);

        var assignedRepairTaskIds = workOrder.RepairTasks.Select(rt => rt.Id).ToHashSet();

        var newRepairTask = await _context.RepairTasks.AsNoTracking()
            .Where(rt => !assignedRepairTaskIds.Contains(rt.Id))
            .FirstAsync();

        var request = new ModifyRepairTaskRequest
        {
            RepairTaskIds = [newRepairTask.Id]
        };

        try
        {
            var response = await _client.PutAsJsonAsync($"/api/v1.0/workorders/{workOrder.Id}/repair-task", request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            await _context.WorkOrders
                 .Where(w => w.Id == workOrder.Id)
                 .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task UpdateRepairTasks_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var workOrderId = Guid.NewGuid();

        var request = new ModifyRepairTaskRequest
        {
            RepairTaskIds = [Guid.NewGuid()]
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/workorders/{workOrderId}/repair-task", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteWorkOrder_WithValidId_ShouldDeleteWorkOrder()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
          .ForToday()
          .WithRepairTasks(await _context.RepairTasks.Take(1).ToListAsync())
          .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
          .WithLabor(TestUsers.Labor01.Id)
          .Build();

        _context.WorkOrders.Add(workOrder);

        await _context.SaveChangesAsync(default);

        try
        {
            var response = await _client.DeleteAsync($"/api/v1.0/workorders/{workOrder.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            await _context.WorkOrders
                 .Where(w => w.Id == workOrder.Id)
                 .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task DeleteWorkOrder_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/api/v1.0/workorders/{nonExistentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteWorkOrder_WithoutManagerRole_ShouldReturnForbidden()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var workOrderId = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/api/v1.0/workorders/{workOrderId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetSchedule_WithSpecificDate_ShouldReturnSchedule()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1.0/workorders/schedule?date={today:yyyy-MM-dd}");

        request.Headers.Add("X-TimeZone", "Eastern Standard Time");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ScheduleDto>();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetSchedule_WithLaborFilter_ShouldReturnFilteredSchedule()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Labor01);

        _client.SetAuthorizationHeader(token);

        var workOrder = WorkOrderTestDataBuilder.Create()
            .ForToday()
            .WithRepairTasks(await _context.RepairTasks.Take(1).ToListAsync())
            .WithVehicle(_context.Vehicles.FirstOrDefault()!.Id)
            .WithLabor(Guid.Parse(TestUsers.Labor01.Id))
            .Build();

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(default);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/v1.0/workorders/schedule?date={today:yyyy-MM-dd}&laborId={TestUsers.Labor01.Id}");

            request.Headers.Add("X-TimeZone", "Eastern Standard Time");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<ScheduleDto>();

            Assert.NotNull(result);
            Assert.NotNull(result.Spots);
        }
        finally
        {
            _context.WorkOrders.Remove(workOrder);
            await _context.SaveChangesAsync(default);
        }
    }

    [Fact]
    public async Task AssignLabor_WithNonExistingWorkOrder_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);

        _client.SetAuthorizationHeader(token);

        var request = new AssignLaborRequest { LaborId = Guid.Parse(TestUsers.Labor01.Id) };

        var nonExistentId = Guid.NewGuid();

        var response = await _client.PutAsJsonAsync($"/api/v1.0/workorders/{nonExistentId}/labor", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AssignLabor_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var request = new AssignLaborRequest { LaborId = Guid.Parse(TestUsers.Labor01.Id) };
        var workOrderId = Guid.NewGuid();

        var response = await _client.PutAsJsonAsync($"/api/v1.0/workorders/{workOrderId}/labor", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWorkOrderState_WithInvalidWorkOrderId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var request = new UpdateWorkOrderStateRequest
        {
            State = Contracts.Common.WorkOrderState.InProgress
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/workorders/{Guid.NewGuid()}/state", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWorkOrderState_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var request = new UpdateWorkOrderStateRequest
        {
            State = Contracts.Common.WorkOrderState.InProgress
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/workorders/{Guid.NewGuid()}/state", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRepairTasks_WithInvalidWorkOrderId_ShouldReturnNotFound()
    {
        var token = await _client.GenerateTokenAsync(TestUsers.Manager);
        _client.SetAuthorizationHeader(token);

        var request = new ModifyRepairTaskRequest
        {
            RepairTaskIds = [Guid.NewGuid()]
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/workorders/{Guid.NewGuid()}/repair-task", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRepairTasks_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var request = new ModifyRepairTaskRequest
        {
            RepairTaskIds = [Guid.NewGuid()]
        };

        var response = await _client.PutAsJsonAsync($"/api/v1.0/workorders/{Guid.NewGuid()}/repair-task", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteWorkOrder_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.DeleteAsync($"/api/v1.0/workorders/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSchedule_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1.0/workorders/schedule?date={today:yyyy-MM-dd}");

        request.Headers.Add("X-TimeZone", "Eastern Standard Time");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}