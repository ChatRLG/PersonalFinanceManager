using System.Net.Http.Json;
using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public class ApiClient : IApiClient
{
	private readonly HttpClient _http;
	private readonly ILogger<ApiClient> _logger;

	public ApiClient(HttpClient http, ILogger<ApiClient> logger)
	{
		_http = http;
		_logger = logger;
	}

	public async Task<ApiResult<T>> GetAsync<T>(string url)
	{
		try
		{
			var response = await _http.GetAsync(url);
			if (response.IsSuccessStatusCode)
			{
				var data = await response.Content.ReadFromJsonAsync<T>();
				return ApiResult<T>.Success(data!);
			}

			var error = await response.Content.ReadAsStringAsync();
			_logger.LogWarning("GET {Url} failed: {Status} — {Error}", url, response.StatusCode, error);
			return ApiResult<T>.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "GET {Url} exception", url);
			return ApiResult<T>.Failure(ex.Message);
		}
	}

	public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest data)
	{
		try
		{
			var response = await _http.PostAsJsonAsync(url, data);
			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadFromJsonAsync<TResponse>();
				return ApiResult<TResponse>.Success(result!);
			}

			var error = await response.Content.ReadAsStringAsync();
			_logger.LogWarning("POST {Url} failed: {Status} — {Error}", url, response.StatusCode, error);
			return ApiResult<TResponse>.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "POST {Url} exception", url);
			return ApiResult<TResponse>.Failure(ex.Message);
		}
	}

	public async Task<ApiResult> PostAsync<TRequest>(string url, TRequest data)
	{
		try
		{
			var response = await _http.PostAsJsonAsync(url, data);
			if (response.IsSuccessStatusCode)
				return ApiResult.Success();

			var error = await response.Content.ReadAsStringAsync();
			_logger.LogWarning("POST {Url} failed: {Status} — {Error}", url, response.StatusCode, error);
			return ApiResult.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "POST {Url} exception", url);
			return ApiResult.Failure(ex.Message);
		}
	}

	public async Task<ApiResult> PutAsync(string url)
	{
		try
		{
			var response = await _http.PutAsync(url, null);
			if (response.IsSuccessStatusCode)
				return ApiResult.Success();

			var error = await response.Content.ReadAsStringAsync();
			_logger.LogWarning("PUT {Url} failed: {Status} — {Error}", url, response.StatusCode, error);
			return ApiResult.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "PUT {Url} exception", url);
			return ApiResult.Failure(ex.Message);
		}
	}

	public async Task<ApiResult> PutAsync<TRequest>(string url, TRequest data)
	{
		try
		{
			var response = await _http.PutAsJsonAsync(url, data);
			if (response.IsSuccessStatusCode)
				return ApiResult.Success();

			var error = await response.Content.ReadAsStringAsync();
			_logger.LogWarning("PUT {Url} failed: {Status} — {Error}", url, response.StatusCode, error);
			return ApiResult.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "PUT {Url} exception", url);
			return ApiResult.Failure(ex.Message);
		}
	}

	public async Task<ApiResult> DeleteAsync(string url)
	{
		try
		{
			var response = await _http.DeleteAsync(url);
			if (response.IsSuccessStatusCode)
				return ApiResult.Success();

			var error = await response.Content.ReadAsStringAsync();
			_logger.LogWarning("DELETE {Url} failed: {Status} — {Error}", url, response.StatusCode, error);
			return ApiResult.Failure(error);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "DELETE {Url} exception", url);
			return ApiResult.Failure(ex.Message);
		}
	}
}
