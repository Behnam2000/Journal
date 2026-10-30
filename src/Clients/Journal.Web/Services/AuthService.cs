using System.Diagnostics;
using Blazored.LocalStorage;
using Journal.Web.Auth;
using Journal.Web.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Journal.Web.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
        return await ProcessAuthResponseAsync(response);
    }

    public async Task<string?> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
        return await ProcessAuthResponseAsync(response);
    }

    public async Task LogoutAsync()
    {
        var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
        if (!string.IsNullOrEmpty(refreshToken))
        {
            // Call the API to revoke the refresh token in the PostgreSQL database
            await _httpClient.PostAsJsonAsync("api/auth/logout", new { RefreshToken = refreshToken });
        }

        // Clear local storage and notify the UI to update
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        ((JwtAuthStateProvider)_authStateProvider).NotifyUserLogout();
    }

    private async Task<string?> ProcessAuthResponseAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            // Return the error string from the API (e.g., "Invalid credentials")
            return await response.Content.ReadAsStringAsync();
        }

        var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (authResult != null)
        {
            // Save tokens to browser storage
            await _localStorage.SetItemAsync("authToken", authResult.Token);
            await _localStorage.SetItemAsync("refreshToken", authResult.RefreshToken);

            // Trigger the UI to recognize the logged-in state
            ((JwtAuthStateProvider)_authStateProvider).NotifyUserAuthentication(authResult.Token);
            return null; // Null signifies success
        }

        return "An unkown error occurred during authentication";

    }

}