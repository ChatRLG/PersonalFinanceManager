using System.Net;
using System.Net.Http.Json;
using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public class ApiClient : IApiClient
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<ApiClient> _logger;

	public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
	{
		_httpClient = httpClient;
		_logger = logger;
	}

	public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
	{
		try
		{
			var response = await _httpClient.GetAsync(endpoint);
			return await HandleResponse<T>(response);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "GET {Endpoint} failed", endpoint);
			return ErrorResponse<T>("Unable to connect to the server. Please try again.");
		}
	}

	public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
	{
		try
		{
			var response = await _httpClient.PostAsJsonAsync(endpoint, data);
			return await HandleResponse<T>(response);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "POST {Endpoint} failed", endpoint);
			return ErrorResponse<T>("Unable to connect to the server. Please try again.");
		}
	}

	public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
	{
		try
		{
			var response = await _httpClient.PutAsJsonAsync(endpoint, data);
			return await HandleResponse<T>(response);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "PUT {Endpoint} failed", endpoint);
			return ErrorResponse<T>("Unable to connect to the server. Please try again.");
		}
	}

	public async Task<ApiResponse> DeleteAsync(string endpoint)
	{
		try
		{
			var response = await _httpClient.DeleteAsync(endpoint);

			if (response.IsSuccessStatusCode)
				return new ApiResponse { IsSuccess = true, Message = "Deleted successfully" };

			var errorContent = await response.Content.ReadAsStringAsync();
			return new ApiResponse
			{
				IsSuccess = false,
				Message = response.StatusCode == HttpStatusCode.NotFound
					? "Resource not found"
					: $"Delete failed: {errorContent}"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "DELETE {Endpoint} failed", endpoint);
			return new ApiResponse
			{
				IsSuccess = false,
				Message = "Unable to connect to the server."
			};
		}
	}

	// ─────────────────────── Helpers ───────────────────────

	private static async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
	{
		if (response.IsSuccessStatusCode)
		{
			var data = await response.Content.ReadFromJsonAsync<T>();
			return new ApiResponse<T> { IsSuccess = true, Data = data };
		}

		var error = await response.Content.ReadAsStringAsync();

		return response.StatusCode switch
		{
			HttpStatusCode.Unauthorized => ErrorResponse<T>("Session expired. Please log in again."),
			HttpStatusCode.Forbidden => ErrorResponse<T>("You do not have permission to perform this action."),
			HttpStatusCode.NotFound => ErrorResponse<T>("The requested resource was not found."),
			HttpStatusCode.BadRequest => ErrorResponse<T>(error),
			_ => ErrorResponse<T>($"Server error ({(int)response.StatusCode}): {error}")
		};
	}

	private static ApiResponse<T> ErrorResponse<T>(string message)
		=> new() { IsSuccess = false, Message = message };
}
