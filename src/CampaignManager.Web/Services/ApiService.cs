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

    private async Task SetAuthHeaderAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        await SetAuthHeaderAsync();
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
        await SetAuthHeaderAsync();
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
        await SetAuthHeaderAsync();
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
        await SetAuthHeaderAsync();
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
