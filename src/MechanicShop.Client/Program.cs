using Blazored.LocalStorage;

using MechanicShop.Client.Hubs;
using MechanicShop.Client.Identity;
using MechanicShop.Client.Services;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

builder.Services.AddScoped(
    sp => (IAccountManagement)sp.GetRequiredService<AuthenticationStateProvider>());

builder.Services.AddTransient<BearerTokenHandler>();

builder.Services.AddScoped<TimeZoneService>();

builder.Services.AddScoped<WorkOrderHubClient>();

builder.Services.AddHttpClient(
    "MechanicShopClient",
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BearerTokenHandler>();

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<ServiceApi>();

await builder.Build().RunAsync();