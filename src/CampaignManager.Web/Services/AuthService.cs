using System.Net.Http.Json;
using Blazored.LocalStorage;
using CampaignManager.Shared.DTOs;

namespace CampaignManager.Web.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
    Task<UserDto?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<Guid?> GetSelectedProjectIdAsync();
    Task SetSelectedProjectAsync(Guid projectId, string projectName);
    Task<string?> GetSelectedProjectNameAsync();
    Task ClearSelectedProjectAsync();
    event Action? OnProjectChanged;
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly CustomAuthStateProvider _authStateProvider;

    public event Action? OnProjectChanged;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage, 
        Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = (CustomAuthStateProvider)authStateProvider;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            
            if (loginResponse?.Success == true && !string.IsNullOrEmpty(loginResponse.Token))
            {
                await _localStorage.SetItemAsync("authToken", loginResponse.Token);
                await _localStorage.SetItemAsync("user", loginResponse.User);
                
                // Clear any previously selected project on new login
                await _localStorage.RemoveItemAsync("selectedProjectId");
                await _localStorage.RemoveItemAsync("selectedProjectName");
                
                _authStateProvider.NotifyAuthStateChanged();
            }
            
            return loginResponse ?? new LoginResponse { Success = false, ErrorMessage = "Login failed" };
        }
        catch (Exception ex)
        {
            return new LoginResponse { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            var registerResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            
            if (registerResponse?.Success == true && !string.IsNullOrEmpty(registerResponse.Token))
            {
                await _localStorage.SetItemAsync("authToken", registerResponse.Token);
                await _localStorage.SetItemAsync("user", registerResponse.User);
                _authStateProvider.NotifyAuthStateChanged();
            }
            
            return registerResponse ?? new LoginResponse { Success = false, ErrorMessage = "Registration failed" };
        }
        catch (Exception ex)
        {
            return new LoginResponse { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("user");
        await _localStorage.RemoveItemAsync("selectedProjectId");
        await _localStorage.RemoveItemAsync("selectedProjectName");
        _authStateProvider.NotifyAuthStateChanged();
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        return await _localStorage.GetItemAsync<UserDto?>("user");
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        return !string.IsNullOrEmpty(token);
    }

    public async Task<Guid?> GetSelectedProjectIdAsync()
    {
        var projectIdStr = await _localStorage.GetItemAsync<string>("selectedProjectId");
        if (Guid.TryParse(projectIdStr, out var projectId))
        {
            return projectId;
        }
        return null;
    }

    public async Task SetSelectedProjectAsync(Guid projectId, string projectName)
    {
        await _localStorage.SetItemAsync("selectedProjectId", projectId.ToString());
        await _localStorage.SetItemAsync("selectedProjectName", projectName);
        OnProjectChanged?.Invoke();
    }

    public async Task<string?> GetSelectedProjectNameAsync()
    {
        return await _localStorage.GetItemAsync<string>("selectedProjectName");
    }

    public async Task ClearSelectedProjectAsync()
    {
        await _localStorage.RemoveItemAsync("selectedProjectId");
        await _localStorage.RemoveItemAsync("selectedProjectName");
        OnProjectChanged?.Invoke();
    }
}
