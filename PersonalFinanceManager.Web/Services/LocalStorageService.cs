using Microsoft.JSInterop;

namespace PersonalFinanceManager.Web.Services;

public class LocalStorageService : ILocalStorageService
{
	private readonly IJSRuntime _jsRuntime;

	public LocalStorageService(IJSRuntime jsRuntime)
	{
		_jsRuntime = jsRuntime;
	}

	public async Task<string?> GetItemAsync(string key)
	{
		return await _jsRuntime.InvokeAsync<string?>("localStorageHelper.getItem", key);
	}

	public async Task SetItemAsync(string key, string value)
	{
		await _jsRuntime.InvokeVoidAsync("localStorageHelper.setItem", key, value);
	}

	public async Task RemoveItemAsync(string key)
	{
		await _jsRuntime.InvokeVoidAsync("localStorageHelper.removeItem", key);
	}

	public async Task ClearAsync()
	{
		await _jsRuntime.InvokeVoidAsync("localStorageHelper.clear");
	}
}