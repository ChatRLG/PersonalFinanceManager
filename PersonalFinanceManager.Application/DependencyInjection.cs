using Microsoft.Extensions.DependencyInjection;
using PersonalFinanceManager.Application.Accounts;
using PersonalFinanceManager.Application.Auth;
using PersonalFinanceManager.Application.Budgets;
using PersonalFinanceManager.Application.Categories;
using PersonalFinanceManager.Application.Dashboard;
using PersonalFinanceManager.Application.Transactions;

namespace PersonalFinanceManager.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddScoped<IAuthAppService, AuthAppService>();
		services.AddScoped<AccountAppService>();
		services.AddScoped<CategoryAppService>();
		services.AddScoped<TransactionAppService>();
		services.AddScoped<BudgetAppService>();
		services.AddScoped<DashboardAppService>();
		return services;
	}
}
