//namespace PersonalFinanceManager.Web.Services;

//public enum ToastLevel
//{
//	Success,
//	Warning,
//	Error,
//	Info
//}

//public class ToastMessage
//{
//	public string Title { get; set; } = string.Empty;
//	public string Message { get; set; } = string.Empty;
//	public ToastLevel Level { get; set; }
//	public DateTime CreatedAt { get; set; } = DateTime.Now;
//	public Guid Id { get; set; } = Guid.NewGuid();
//}

//public class ToastService : IDisposable
//{
//	public event Action<ToastMessage>? OnShow;
//	public event Action<Guid>? OnHide;

//	public void ShowSuccess(string message, string title = "Success")
//		=> Show(title, message, ToastLevel.Success);

//	public void ShowError(string message, string title = "Error")
//		=> Show(title, message, ToastLevel.Error);

//	public void ShowWarning(string message, string title = "Warning")
//		=> Show(title, message, ToastLevel.Warning);

//	public void ShowInfo(string message, string title = "Info")
//		=> Show(title, message, ToastLevel.Info);

//	private void Show(string title, string message, ToastLevel level)
//	{
//		OnShow?.Invoke(new ToastMessage
//		{
//			Title = title,
//			Message = message,
//			Level = level
//		});
//	}

//	public void Hide(Guid toastId) => OnHide?.Invoke(toastId);

//	public void Dispose() { }
//}


namespace PersonalFinanceManager.Web.Services;

public class ToastService
{
	public event Action<ToastItem>? OnShow;

	public void ShowSuccess(string message, int durationMs = 4000)
		=> OnShow?.Invoke(new ToastItem(message, ToastType.Success, durationMs));

	public void ShowError(string message, int durationMs = 6000)
		=> OnShow?.Invoke(new ToastItem(message, ToastType.Error, durationMs));

	public void ShowWarning(string message, int durationMs = 5000)
		=> OnShow?.Invoke(new ToastItem(message, ToastType.Warning, durationMs));

	public void ShowInfo(string message, int durationMs = 4000)
		=> OnShow?.Invoke(new ToastItem(message, ToastType.Info, durationMs));
}

public class ToastItem
{
	public string Message { get; }
	public ToastType Type { get; }
	public int DurationMs { get; }
	public Guid Id { get; } = Guid.NewGuid();

	public ToastItem(string message, ToastType type, int durationMs)
	{
		Message = message;
		Type = type;
		DurationMs = durationMs;
	}
}

public enum ToastType
{
	Success,
	Error,
	Warning,
	Info
}
