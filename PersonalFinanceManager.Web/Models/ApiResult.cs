namespace PersonalFinanceManager.Web.Models;

public class ApiResult<T>
{
	public bool IsSuccess { get; set; }
	public T? Data { get; set; }
	public string? Message { get; set; }

	public static ApiResult<T> Success(T data)
		=> new() { IsSuccess = true, Data = data };

	public static ApiResult<T> Failure(string message)
		=> new() { IsSuccess = false, Message = message };
}

public class ApiResult
{
	public bool IsSuccess { get; set; }
	public string? Message { get; set; }

	public static ApiResult Success()
		=> new() { IsSuccess = true };

	public static ApiResult Failure(string message)
		=> new() { IsSuccess = false, Message = message };
}