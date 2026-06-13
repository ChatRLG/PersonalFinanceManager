using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

/// <summary>
/// Low-level typed HTTP client for communicating with the API.
/// </summary>
public interface IApiClient
{
    Task<ApiResponse<T>> GetAsync<T>(string endpoint);
    Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data);
    Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data);
    Task<ApiResponse> DeleteAsync(string endpoint);
}
