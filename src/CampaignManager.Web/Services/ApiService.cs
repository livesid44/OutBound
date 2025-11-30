using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using CampaignManager.Shared.DTOs;

namespace CampaignManager.Web.Services;

public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object? data = null);
    Task<T?> PutAsync<T>(string endpoint, object? data = null);
    Task<bool> DeleteAsync(string endpoint);
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
            return await _httpClient.GetFromJsonAsync<T>($"api/{endpoint}");
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
                return await response.Content.ReadFromJsonAsync<T>();
            }
            return default;
        }
        catch (Exception)
        {
            return default;
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
                return await response.Content.ReadFromJsonAsync<T>();
            }
            return default;
        }
        catch (Exception)
        {
            return default;
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
