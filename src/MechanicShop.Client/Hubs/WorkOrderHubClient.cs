using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;

namespace MechanicShop.Client.Hubs;

public sealed class WorkOrderHubClient : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private Func<Task>? _onWorkOrdersChanged;
    private bool _isStarted;
    private bool _isDisposed;

    public WorkOrderHubClient(IWebAssemblyHostEnvironment env)
    {
        var baseUrl = env.BaseAddress;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}hubs/workorders")
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On("WorkOrdersChanged", async () =>
        {
            if (!_isDisposed && _onWorkOrdersChanged is not null)
            {
                await _onWorkOrdersChanged.Invoke();
            }
        });
    }

    public async Task StartAsync(Func<Task> onWorkOrdersChanged)
    {
        if (_isDisposed)
        {
            return;
        }

        _onWorkOrdersChanged = onWorkOrdersChanged;

        if (_isStarted)
        {
            return;
        }

        await _hubConnection.StartAsync();
        _isStarted = true;
    }

    public void StopListening()
    {
        _onWorkOrdersChanged = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (_hubConnection.State is HubConnectionState.Connected or HubConnectionState.Connecting)
        {
            await _hubConnection.StopAsync();
        }

        await _hubConnection.DisposeAsync();
    }
}