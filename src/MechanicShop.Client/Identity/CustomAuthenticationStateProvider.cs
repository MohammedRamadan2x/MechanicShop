using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

using Blazored.LocalStorage;

using Microsoft.AspNetCore.Components.Authorization;

namespace MechanicShop.Client.Identity;

public class CustomAuthenticationStateProvider(
    ILocalStorageService localStorageService,
    IHttpClientFactory httpClientFactory,
    ILogger<CustomAuthenticationStateProvider> logger) : AuthenticationStateProvider, IAccountManagement
{
    private readonly ILocalStorageService _localStorageService = localStorageService;
    private readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly ClaimsPrincipal unauthenticated = new(new ClaimsIdentity());

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    private bool authenticated = false;

    public async Task<FormResult> LoginAsync(string email, string password)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("MechanicShopClient");
            var result = await httpClient.PostAsJsonAsync(
                "identity/token/generate", new
                {
                    email,
                    password
                });

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadFromJsonAsync<TokenResponse>();
                await _localStorageService.SetItemAsync("authResult", response);

                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

                return new FormResult { Succeeded = true };
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "App error");
        }

        return new FormResult
        {
            Succeeded = false,
            ErrorList = ["Invalid email and/or password."]
        };
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        authenticated = false;

        // default to not authenticated
        var user = unauthenticated;

        try
        {
            var httpClient = _httpClientFactory.CreateClient("MechanicShopClient");

            // the user info endpoint is secured, so if the user isn't logged in this will fail
            var userResponse = await httpClient.GetAsync("identity/current-user/claims");

            // throw if user info wasn't retrieved
            userResponse.EnsureSuccessStatusCode();

            // user is authenticated,so let's build their authenticated identity
            var userJson = await userResponse.Content.ReadAsStringAsync();

            var userInfo = JsonSerializer.Deserialize<UserInfo>(userJson, jsonSerializerOptions);

            if (userInfo != null)
            {
                // in this example app, name and email are the same
                List<Claim> claims =
                [
                    new (ClaimTypes.Name, userInfo.Email),
                    new (ClaimTypes.NameIdentifier, userInfo.UserId),
                    new (ClaimTypes.Email, userInfo.Email),
                ];

                foreach (var role in userInfo.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                claims.AddRange(userInfo.Claims);

                var id = new ClaimsIdentity(claims, nameof(CustomAuthenticationStateProvider));

                user = new ClaimsPrincipal(id);

                authenticated = true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "App error");
        }

        // return the state
        return new AuthenticationState(user);
    }

    public async Task LogoutAsync()
    {
        await _localStorageService.RemoveItemAsync("authResult");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<bool> CheckAuthenticatedAsync()
    {
        await GetAuthenticationStateAsync();
        return authenticated;
    }

    public async Task<TokenResponse?> RefreshTokenAsync()
    {
        var authResult = await _localStorageService.GetItemAsync<TokenResponse>("authResult");

        if (authResult?.RefreshToken is null)
        {
            return null; // No refresh token available
        }

        var httpClient = _httpClientFactory.CreateClient("MechanicShopClient");
        var refreshResponse = await httpClient.PostAsJsonAsync("identity/token/refresh-token", new
        {
            ExpiredAccessToken = authResult.AccessToken,
            authResult.RefreshToken
        });

        if (!refreshResponse.IsSuccessStatusCode)
        {
            return null; // Refresh failed
        }

        var newTokenResponse = await refreshResponse.Content.ReadFromJsonAsync<TokenResponse>();

        if (newTokenResponse is null || newTokenResponse.ExpiresOnUtc <= DateTime.UtcNow)
        {
            return null; // Avoid storing expired tokens
        }

        await _localStorageService.SetItemAsync("authResult", newTokenResponse);
        return newTokenResponse;
    }

    public async Task<TokenResponse?> LoadAccessTokenFromStorage()
    {
        return await _localStorageService.GetItemAsync<TokenResponse>("authResult");
    }
}