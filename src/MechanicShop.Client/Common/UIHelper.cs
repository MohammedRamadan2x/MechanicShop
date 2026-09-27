using MechanicShop.Contracts.Common;

using Microsoft.AspNetCore.Components;

namespace MechanicShop.Client.Common;

public static class UIHelper
{
    public static MarkupString GetStatusBadge(WorkOrderState? state)
    {
        return state switch
        {
            WorkOrderState.Scheduled => (MarkupString)"<span class='badge mx-1 text-warning'><i class='bi bi-hourglass-split'></i> Scheduled</span>",
            WorkOrderState.InProgress => (MarkupString)"<span class='badge mx-1 text-secondary'><i class='bi bi-tools'></i> InProgress</span>",
            WorkOrderState.Completed => (MarkupString)"<span class='badge mx-1 text-success'><i class='bi bi-check-all'></i> Completed</span>",
            WorkOrderState.Cancelled => (MarkupString)"<span class='badge mx-1 text-danger'><i class='bi bi-x-circle'></i> Cancelled</span>",
            _ => (MarkupString)"<span class='badge mx-1 bg-secondary'><i class='bi bi-question-circle'></i> Unknown</span>"
        };
    }

    public static MarkupString GetServiceBadge(string? serviceName)
    {
        return serviceName switch
        {
            "Oil Change" => (MarkupString)"<span class='badge mx-1 text-bg-warning'><i class='bi bi-droplet'></i> Oil Change</span>",
            "Brake Replacement" => (MarkupString)"<span class='badge mx-1 text-bg-danger'><i class='bi bi-exclamation-triangle'></i> Brake Replacement</span>",
            "Tire Rotation" => (MarkupString)"<span class='badge mx-1 bg-info text-text-dark'><i class='bi bi-arrow-repeat'></i> Tire Rotation</span>",
            "Battery Replacement" => (MarkupString)"<span class='badge mx-1 text-bg-primary'><i class='bi bi-battery'></i> Battery Replacement</span>",
            "Wheel Alignment" => (MarkupString)"<span class='badge mx-1 text-bg-success'><i class='bi bi-compass'></i> Wheel Alignment</span>",
            "Air Conditioning Recharge" => (MarkupString)"<span class='badge mx-1 text-bg-secondary'><i class='bi bi-wind'></i> Air Conditioning Recharge</span>",
            "Spark Plug Replacement" => (MarkupString)"<span class='badge mx-1 text-bg-warning'><i class='bi bi-lightning'></i> Spark Plug Replacement</span>",
            "Engine Diagnostic" => (MarkupString)"<span class='badge mx-1 text-bg-info'><i class='bi bi-cpu'></i> Engine Diagnostic</span>",
            "Timing Belt Replacement" => (MarkupString)"<span class='badge mx-1 text-bg-primary'><i class='bi bi-gear'></i> Timing Belt Replacement</span>",
            "Transmission Fluid Change" => (MarkupString)"<span class='badge mx-1 text-bg-danger'><i class='bi bi-droplet-half'></i> Transmission Fluid Change</span>",
            _ => (MarkupString)"<span class='badge mx-1 text-bg-secondary'><i class='bi bi-question-circle'></i> Other Service</span>"
        };
    }

    public static string FormatCurrency(decimal amount)
    {
        return $"$ {amount:N0}";
    }
}