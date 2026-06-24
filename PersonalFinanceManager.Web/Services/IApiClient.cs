using PersonalFinanceManager.Web.Models;

namespace PersonalFinanceManager.Web.Services;

public interface IApiClient
{
	Task<ApiResult<T>> GetAsync<T>(string url);
	Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest data);
	Task<ApiResult> PostAsync<TRequest>(string url, TRequest data);
	Task<ApiResult> PutAsync(string url);
	Task<ApiResult> PutAsync<TRequest>(string url, TRequest data);
	Task<ApiResult> DeleteAsync(string url);
}