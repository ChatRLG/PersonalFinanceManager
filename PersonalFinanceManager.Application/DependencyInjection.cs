using Microsoft.Extensions.DependencyInjection;
using PersonalFinanceManager.Application.Auth;

namespace PersonalFinanceManager.Application;

/// <summary>
/// Registers the Application layer's use-case services into the DI container.
/// Mirrors Infrastructure's AddInfrastructure(); called from the API's Program.cs.
/// </summary>
public static class DependencyInjection
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddScoped<IAuthAppService, AuthAppService>();
		return services;
	}
}
