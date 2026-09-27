using Microsoft.JSInterop;

namespace MechanicShop.Client.Services;

public sealed class TimeZoneService(IJSRuntime js)
{
    private readonly IJSRuntime _js = js;

    public async Task<string> GetLocalTimeZoneAsync()
    {
        return await _js.InvokeAsync<string>("getLocalTimeZone");
    }
}