using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using MechanicShop.Client.Extensions;
using MechanicShop.Client.Models;
using MechanicShop.Contracts.Requests.Customers;
using MechanicShop.Contracts.Requests.RepairTasks;
using MechanicShop.Contracts.Requests.WorkOrders;
using MechanicShop.Contracts.Responses;

namespace MechanicShop.Client.Services;

public class ServiceApi(IHttpClientFactory httpClientFactory, TimeZoneService timeZoneService)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("MechanicShopClient");
    private readonly TimeZoneService _timeZoneService = timeZoneService;

    // Customer methods
    public async Task<ApiResult<List<CustomerModel>>> GetCustomersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/customers");

            if (response.IsSuccessStatusCode)
            {
                var customers = await response.Content.ReadFromJsonAsync<List<CustomerModel>>();
                return ApiResult<List<CustomerModel>>.Success(customers ?? []);
            }

            return await HandleErrorResponseAsync<List<CustomerModel>>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<List<CustomerModel>>(ex, "Failed to retrieve customers");
        }
    }

    public async Task<ApiResult<CustomerModel>> GetCustomerAsync(Guid customerId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/customers/{customerId}");

            if (response.IsSuccessStatusCode)
            {
                var customer = await response.Content.ReadFromJsonAsync<CustomerModel>();
                return ApiResult<CustomerModel>.Success(customer!);
            }

            return await HandleErrorResponseAsync<CustomerModel>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<CustomerModel>(ex, $"Failed to retrieve customer {customerId}");
        }
    }

    // Labor methods
    public async Task<ApiResult<List<LaborModel>>> GetLaborsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/labors");

            if (response.IsSuccessStatusCode)
            {
                var labors = await response.Content.ReadFromJsonAsync<List<LaborModel>>();
                return ApiResult<List<LaborModel>>.Success(labors ?? []);
            }

            return await HandleErrorResponseAsync<List<LaborModel>>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<List<LaborModel>>(ex, "Failed to retrieve labors");
        }
    }

    // Repair Task methods
    public async Task<ApiResult<List<RepairTaskModel>>> GetRepairTasksAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/repair-tasks");

            if (response.IsSuccessStatusCode)
            {
                var repairTasks = await response.Content.ReadFromJsonAsync<List<RepairTaskModel>>();
                return ApiResult<List<RepairTaskModel>>.Success(repairTasks ?? []);
            }

            return await HandleErrorResponseAsync<List<RepairTaskModel>>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<List<RepairTaskModel>>(ex, "Failed to retrieve repair tasks");
        }
    }

    // Work Order methods
    public async Task<ApiResult<PaginatedList<WorkOrderListItemModel>>> GetWorkOrdersAsync(
        WorkOrderFilterRequest request,
        PageRequest pageRequest,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queryString = BuildQueryString(request, pageRequest);
            var url = $"api/v1/WorkOrders?{queryString}";

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var workOrders = await response.Content.ReadFromJsonAsync<PaginatedList<WorkOrderListItemModel>>(cancellationToken: cancellationToken);

                workOrders?.Items.ForEach(item => item.AdjustTimeToLocal());

                return ApiResult<PaginatedList<WorkOrderListItemModel>>.Success(workOrders!);
            }

            return await HandleErrorResponseAsync<PaginatedList<WorkOrderListItemModel>>(response);
        }
        catch (OperationCanceledException)
        {
            return ApiResult<PaginatedList<WorkOrderListItemModel>>.Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<PaginatedList<WorkOrderListItemModel>>(ex, "Failed to retrieve work orders");
        }
    }

    public async Task<ApiResult<WorkOrderModel>> GetWorkOrderByIdAsync(Guid workOrderId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/WorkOrders/{workOrderId}");

            if (response.IsSuccessStatusCode)
            {
                var workOrder = await response.Content.ReadFromJsonAsync<WorkOrderModel>();
                workOrder?.AdjustTimeToLocal();

                return ApiResult<WorkOrderModel>.Success(workOrder!);
            }

            return await HandleErrorResponseAsync<WorkOrderModel>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<WorkOrderModel>(ex, $"Failed to retrieve work order {workOrderId}");
        }
    }

    public async Task<ApiResult> CreateWorkOrderAsync(CreateWorkOrderRequest workOrderRequest)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/WorkOrders", workOrderRequest);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, "Failed to create work order");
        }
    }

    public async Task<ApiResult> RelocateWorkOrderAsync(Guid workOrderId, RelocateWorkOrderRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/WorkOrders/{workOrderId}/relocation", request);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, $"Failed to relocate work order timing for {workOrderId}");
        }
    }

    public async Task<ApiResult> UpdateWorkOrderLaborAsync(Guid workOrderId, AssignLaborRequest laborUpdateRequest)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/WorkOrders/{workOrderId}/labor", laborUpdateRequest);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, $"Failed to update work order labor for {workOrderId}");
        }
    }

    public async Task<ApiResult> UpdateWorkOrderStateAsync(Guid workOrderId, UpdateWorkOrderStateRequest statusUpdateRequest)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/WorkOrders/{workOrderId}/state", statusUpdateRequest);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, $"Failed to update work order state for {workOrderId}");
        }
    }

    public async Task<ApiResult> UpdateWorkOrderRepairTasksAsync(Guid workOrderId, ModifyRepairTaskRequest updateWorkOrderRepairTasks)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/WorkOrders/{workOrderId}/repair-task", updateWorkOrderRepairTasks);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, $"Failed to update work order repair tasks for {workOrderId}");
        }
    }

    public async Task<ApiResult> DeleteWorkOrderAsync(Guid workOrderId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/WorkOrders/{workOrderId}");

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, $"Failed to delete work order {workOrderId}");
        }
    }

    // Vehicle methods
    public async Task<ApiResult<List<VehicleModel>>> GetVehiclesByCustomerIdAsync(Guid customerId)
    {
        try
        {
            var customerResult = await GetCustomerAsync(customerId);

            if (!customerResult.IsSuccess)
            {
                return ApiResult<List<VehicleModel>>.Failure(
                    customerResult.ErrorMessage,
                    customerResult.ErrorDetail,
                    customerResult.StatusCode,
                    customerResult.ValidationErrors);
            }

            var vehicles = customerResult.Data?.Vehicles ?? [];
            return ApiResult<List<VehicleModel>>.Success(vehicles);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<List<VehicleModel>>(ex, $"Failed to retrieve vehicles for customer {customerId}");
        }
    }
    
    // Schedule methods
    public async Task<ApiResult<ScheduleModel>> GetDailySchedule(DateOnly date, Guid? laborId = null)
    {
        try
        {
            var url = $"api/v1/workorders/schedule?date={date:yyyy-MM-dd}";

            if (laborId.HasValue)
            {
                url += $"&laborId={laborId.Value}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var tz = await _timeZoneService.GetLocalTimeZoneAsync();

            request.Headers.Add("X-TimeZone", tz);

            var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                CancellationToken.None)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var schedule = await response.Content
                    .ReadFromJsonAsync<ScheduleModel>()
                    .ConfigureAwait(false);

                if (schedule is null)
                {
                    return ApiResult<ScheduleModel>.Failure("Schedule data is null");
                }

                try
                {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Montreal");

                    foreach (var slot in schedule.Spots.SelectMany(s => s.Slots))
                    {
                        slot.StartAt = TimeZoneInfo.ConvertTime(slot.StartAt, timeZone);
                        slot.EndAt = TimeZoneInfo.ConvertTime(slot.EndAt, timeZone);
                    }
                }
                catch (TimeZoneNotFoundException)
                {
                    return ApiResult<ScheduleModel>.Failure(
                        "Time zone 'America/Montreal' not found on this system.");
                }

                return ApiResult<ScheduleModel>.Success(schedule);
            }

            return await HandleErrorResponseAsync<ScheduleModel>(response)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<ScheduleModel>(
                    ex,
                    $"Failed to retrieve schedule for {date}")
                .ConfigureAwait(false);
        }
    }

    // Settings methods
    public async Task<ApiResult<OperatingHoursResponse>> GetOperateHoursAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/settings/operating-hours");

            if (response.IsSuccessStatusCode)
            {
                var operateHours = await response.Content.ReadFromJsonAsync<OperatingHoursResponse>();

                return ApiResult<OperatingHoursResponse>.Success(operateHours!);
            }

            return await HandleErrorResponseAsync<OperatingHoursResponse>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<OperatingHoursResponse>(ex, "Failed to retrieve operating hours");
        }
    }

    // Invoice methods
    public async Task<ApiResult<InvoiceModel>> IssueInvoiceAsync(Guid workorderId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/v1/invoices/workorders/{workorderId}", null);

            if (response.IsSuccessStatusCode)
            {
                var invoice = await response.Content.ReadFromJsonAsync<InvoiceModel>();

                if (invoice == null)
                {
                    return ApiResult<InvoiceModel>.Failure("Invoice response was null");
                }

                return ApiResult<InvoiceModel>.Success(invoice);
            }

            return await HandleErrorResponseAsync<InvoiceModel>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<InvoiceModel>(ex, $"Failed to issue invoice for work order {workorderId}");
        }
    }

    public async Task<ApiResult<InvoiceModel>> GetInvoiceAsync(Guid invoiceId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/invoices/{invoiceId}");

            if (response.IsSuccessStatusCode)
            {
                var invoice = await response.Content.ReadFromJsonAsync<InvoiceModel>();
                return ApiResult<InvoiceModel>.Success(invoice!);
            }

            return await HandleErrorResponseAsync<InvoiceModel>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<InvoiceModel>(ex, $"Failed to retrieve invoice {invoiceId}");
        }
    }

    public async Task<ApiResult<byte[]>> GetInvoicePdfAsync(Guid invoiceId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/invoices/{invoiceId}/pdf");

            if (response.IsSuccessStatusCode)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                return ApiResult<byte[]>.Success(pdfBytes);
            }

            return await HandleErrorResponseAsync<byte[]>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<byte[]>(ex, $"Failed to retrieve PDF for invoice {invoiceId}");
        }
    }

    public async Task<ApiResult> SettleInvoice(Guid invoiceId)
    {
        try
        {
            var response = await _httpClient.PutAsync($"api/v1/invoices/{invoiceId}/payments", null);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, $"Failed to settle invoice {invoiceId}");
        }
    }

    public async Task<ApiResult<TodayWorkOrderStatsModel>> GetTodayWorkOrderStatsAsync(DateOnly? date = null)
    {
        try
        {
            var url = "api/v1/dashboard/stats";

            if (date.HasValue)
            {
                url += $"?date={date.Value:yyyy-MM-dd}";
            }

            var response = await _httpClient.GetFromJsonAsync<TodayWorkOrderStatsModel>(url);

            return ApiResult<TodayWorkOrderStatsModel>.Success(response!);
        }
        catch (HttpRequestException ex)
        {
            return ApiResult<TodayWorkOrderStatsModel>.Failure(
                "Network error while fetching work order stats.",
                ex.Message,
                (int?)ex.StatusCode ?? 500);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<TodayWorkOrderStatsModel>(ex, "Unexpected error occurred while fetching dashboard stats.");
        }
    }

    public async Task<ApiResult<CustomerModel>> CreateCustomerAsync(CreateCustomerRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/customers", request);

            if (response.IsSuccessStatusCode)
            {
                var customer = await response.Content.ReadFromJsonAsync<CustomerModel>();

                if (customer is null)
                {
                    return ApiResult<CustomerModel>.Failure("Customer response was null.");
                }

                return ApiResult<CustomerModel>.Success(customer);
            }

            return await HandleErrorResponseAsync<CustomerModel>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<CustomerModel>(ex, "Failed to create customer.");
        }
    }

    public async Task<ApiResult> UpdateCustomerAsync(
    Guid customerId,
    UpdateCustomerRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/customers/{customerId}",
                request);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(
                ex,
                $"Failed to update customer {customerId}");
        }
    }

    public async Task<ApiResult> DeleteCustomerAsync(Guid customerId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/customers/{customerId}");

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, $"Failed to delete customer {customerId}");
        }
    }

    public async Task<ApiResult<RepairTaskModel>> CreateRepairTaskAsync(CreateRepairTaskRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/repair-tasks", request);

            if (response.IsSuccessStatusCode)
            {
                var repairTask = await response.Content.ReadFromJsonAsync<RepairTaskModel>();
                return ApiResult<RepairTaskModel>.Success(repairTask!);
            }

            return await HandleErrorResponseAsync<RepairTaskModel>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<RepairTaskModel>(ex, "Failed to create repair task");
        }
    }

    public async Task<ApiResult> UpdateRepairTaskAsync(
        Guid repairTaskId,
        UpdateRepairTaskRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/repair-tasks/{repairTaskId}",
                request);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(
                ex,
                $"Failed to update repair task {repairTaskId}");
        }
    }

    public async Task<ApiResult> DeleteRepairTaskAsync(Guid repairTaskId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/repair-tasks/{repairTaskId}");

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, $"Failed to delete repair task {repairTaskId}");
        }
    }

    // Private helper methods
    private static async Task<ApiResult<T>> HandleErrorResponseAsync<T>(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();

        try
        {
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, options: new() { PropertyNameCaseInsensitive = true });

            if (problemDetails is not null)
            {
                return ApiResult<T>.Failure(
                    message: problemDetails.Title ?? "An error occurred",
                    detail: problemDetails.Detail ?? "Error",
                    statusCode: problemDetails.Status ?? (int)response.StatusCode,
                    validationErrors: problemDetails.Errors);
            }

            return ApiResult<T>.Failure(
                message: GetFriendlyErrorMessage(response.StatusCode),
                detail: content,
                statusCode: (int)response.StatusCode);
        }
        catch (JsonException)
        {
            return ApiResult<T>.Failure(
                message: GetFriendlyErrorMessage(response.StatusCode),
                detail: content,
                statusCode: (int)response.StatusCode);
        }
    }

    private static Task<ApiResult> HandleErrorResponseAsync(HttpResponseMessage response) =>
        HandleErrorResponseAsync<object>(response)
            .ContinueWith(t =>
                ApiResult.Failure(
                    t.Result.ErrorMessage,
                    t.Result.ErrorDetail,
                    t.Result.StatusCode,
                    t.Result.ValidationErrors));

    private static Task<ApiResult<T>> HandleExceptionAsync<T>(Exception ex, string message) =>
      Task.FromResult(ex switch
      {
          HttpRequestException => ApiResult<T>.Failure($"Network error occurred. {message}"),
          TaskCanceledException => ApiResult<T>.Failure($"Request timed out. {message}"),
          _ => ApiResult<T>.Failure($"An unexpected error occurred. {message}")
      });

    private static Task<ApiResult> HandleExceptionAsync(Exception ex, string message) =>
        HandleExceptionAsync<object>(ex, message).ContinueWith(t =>
            ApiResult.Failure(
                t.Result.ErrorMessage,
                t.Result.ErrorDetail,
                t.Result.StatusCode,
                t.Result.ValidationErrors));

    private static string GetFriendlyErrorMessage(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "Invalid request. Please check your input and try again.",
            HttpStatusCode.Unauthorized => "You are not authorized to perform this action.",
            HttpStatusCode.Forbidden => "You don't have permission to perform this action.",
            HttpStatusCode.NotFound => "The requested resource was not found.",
            HttpStatusCode.Conflict => "The operation conflicts with the current state of the resource.",
            HttpStatusCode.UnprocessableEntity => "The request contains invalid data.",
            HttpStatusCode.InternalServerError => "A server error occurred. Please try again later.",
            HttpStatusCode.BadGateway => "Service temporarily unavailable. Please try again later.",
            HttpStatusCode.ServiceUnavailable => "Service temporarily unavailable. Please try again later.",
            HttpStatusCode.GatewayTimeout => "The request timed out. Please try again.",
            _ => "An error occurred while processing your request."
        };
    }

    private static string BuildQueryString(WorkOrderFilterRequest filterRequest, PageRequest pageRequest)
    {
        var queryParams = new List<string>
        {
            $"page={pageRequest.Page}",
            $"pageSize={pageRequest.PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(filterRequest.SearchTerm))
        {
            queryParams.Add($"searchTerm={Uri.EscapeDataString(filterRequest.SearchTerm)}");
        }

        if (!string.IsNullOrWhiteSpace(filterRequest.SortColumn))
        {
            queryParams.Add($"sortColumn={Uri.EscapeDataString(filterRequest.SortColumn)}");
        }

        if (!string.IsNullOrWhiteSpace(filterRequest.SortDirection))
        {
            queryParams.Add($"sortDirection={Uri.EscapeDataString(filterRequest.SortDirection)}");
        }

        if (filterRequest.State.HasValue)
        {
            queryParams.Add($"state={filterRequest.State}");
        }

        if (filterRequest.VehicleId.HasValue && filterRequest.VehicleId != Guid.Empty)
        {
            queryParams.Add($"vehicleId={filterRequest.VehicleId}");
        }

        if (filterRequest.LaborId.HasValue && filterRequest.LaborId != Guid.Empty)
        {
            queryParams.Add($"laborId={filterRequest.LaborId}");
        }

        if (filterRequest.StartDateFrom.HasValue)
        {
            queryParams.Add($"startDateFrom={filterRequest.StartDateFrom:yyyy-MM-ddTHH:mm:ss}");
        }

        if (filterRequest.StartDateTo.HasValue)
        {
            queryParams.Add($"startDateTo={filterRequest.StartDateTo:yyyy-MM-ddTHH:mm:ss}");
        }

        if (filterRequest.EndDateFrom.HasValue)
        {
            queryParams.Add($"endDateFrom={filterRequest.EndDateFrom:yyyy-MM-ddTHH:mm:ss}");
        }

        if (filterRequest.EndDateTo.HasValue)
        {
            queryParams.Add($"endDateTo={filterRequest.EndDateTo:yyyy-MM-ddTHH:mm:ss}");
        }

        if (filterRequest.Spot.HasValue)
        {
            queryParams.Add($"spot={filterRequest.Spot}");
        }

        return string.Join("&", queryParams);
    }
}