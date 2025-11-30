using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using CampaignManager.Shared.DTOs;

namespace CampaignManager.Web.Services;

public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object? data = null);
    Task<T?> PutAsync<T>(string endpoint, object? data = null);
    Task<bool> DeleteAsync(string endpoint);
    Task<(T? Result, string? ErrorMessage)> PostWithErrorAsync<T>(string endpoint, object? data = null);
    Task<(T? Result, string? ErrorMessage)> PutWithErrorAsync<T>(string endpoint, object? data = null);
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public ApiService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    private async Task SetHeadersAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Add selected project ID header for SuperAdmin tenant selection
        var selectedProjectId = await _localStorage.GetItemAsync<string>("selectedProjectId");
        if (!string.IsNullOrEmpty(selectedProjectId))
        {
            // Remove existing header if present
            _httpClient.DefaultRequestHeaders.Remove("X-Project-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Project-Id", selectedProjectId);
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        await SetHeadersAsync();
        try
        {
            var response = await _httpClient.GetAsync($"api/{endpoint}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(content) && content.TrimStart().StartsWith("{") || content.TrimStart().StartsWith("["))
                {
                    return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            return default;
        }
        catch (Exception)
        {
            return default;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object? data = null)
    {
        await SetHeadersAsync();
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/{endpoint}", data);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(content) && (content.TrimStart().StartsWith("{") || content.TrimStart().StartsWith("[")))
                {
                    return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            return default;
        }
        catch (Exception)
        {
            return default;
        }
    }

    public async Task<(T? Result, string? ErrorMessage)> PostWithErrorAsync<T>(string endpoint, object? data = null)
    {
        await SetHeadersAsync();
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/{endpoint}", data);
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrEmpty(content) && (content.TrimStart().StartsWith("{") || content.TrimStart().StartsWith("[")))
                {
                    var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (result, null);
                }
                return (default, null);
            }
            else
            {
                // Try to parse error message from response
                try
                {
                    if (!string.IsNullOrEmpty(content) && content.TrimStart().StartsWith("{"))
                    {
                        var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return (default, errorResponse?.Message ?? $"Error: {response.StatusCode}");
                    }
                }
                catch { }
                return (default, $"Error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return (default, ex.Message);
        }
    }

    public async Task<T?> PutAsync<T>(string endpoint, object? data = null)
    {
        await SetHeadersAsync();
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/{endpoint}", data);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(content) && (content.TrimStart().StartsWith("{") || content.TrimStart().StartsWith("[")))
                {
                    return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            return default;
        }
        catch (Exception)
        {
            return default;
        }
    }

    public async Task<(T? Result, string? ErrorMessage)> PutWithErrorAsync<T>(string endpoint, object? data = null)
    {
        await SetHeadersAsync();
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/{endpoint}", data);
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrEmpty(content) && (content.TrimStart().StartsWith("{") || content.TrimStart().StartsWith("[")))
                {
                    var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (result, null);
                }
                return (default, null);
            }
            else
            {
                // Try to parse error message from response
                try
                {
                    if (!string.IsNullOrEmpty(content) && content.TrimStart().StartsWith("{"))
                    {
                        var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return (default, errorResponse?.Message ?? $"Error: {response.StatusCode}");
                    }
                }
                catch { }
                return (default, $"Error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return (default, ex.Message);
        }
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        await SetHeadersAsync();
        try
        {
            var response = await _httpClient.DeleteAsync($"api/{endpoint}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
