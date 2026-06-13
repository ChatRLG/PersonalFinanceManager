namespace PersonalFinanceManager.Web.Models;

/// <summary>
/// Generic wrapper for all API responses.
/// </summary>
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ApiResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
}
