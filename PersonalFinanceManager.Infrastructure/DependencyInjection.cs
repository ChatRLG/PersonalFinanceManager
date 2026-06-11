using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalFinanceManager.Core.Interfaces;
using PersonalFinanceManager.Infrastructure.Data;
using PersonalFinanceManager.Infrastructure.Repositories;

namespace PersonalFinanceManager.Infrastructure;

/// <summary>
/// Extension method that wires up all Infrastructure services 
/// into the DI container. Called from Program.cs.
/// </summary>
public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		// ──────────── DbContext ────────────
		//var connectionString = configuration.GetConnectionString("DefaultConnection")
		//                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

		//services.AddDbContext<AppDBContext>(options =>
		//	options.UseSqlServer(connectionString, sqlOptions =>
		//	{
		//		sqlOptions.MigrationsAssembly(typeof(AppDBContext).Assembly.FullName);
		//		sqlOptions.EnableRetryOnFailure(
		//			maxRetryCount: 3,
		//			maxRetryDelay: TimeSpan.FromSeconds(10),
		//			errorNumbersToAdd: null);
		//	}));

		services.AddDbContext<AppDBContext>(options =>
			options.UseSqlServer(
				configuration.GetConnectionString("DefaultConnection"),
				sqlOptions =>
				{
					sqlOptions.MigrationsAssembly(typeof(AppDBContext).Assembly.FullName);
					sqlOptions.EnableRetryOnFailure(3);
				}));

		// ──────────── Repositories ────────────
		services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
		services.AddScoped<IUserRepository, UserRepository>();
		services.AddScoped<IAccountRepository, AccountRepository>();
		services.AddScoped<ITransactionRepository, TransactionRepository>();
		services.AddScoped<ICategoryRepository, CategoryRepository>();
		services.AddScoped<IBudgetRepository, BudgetRepository>();

		// ──────────── Unit of Work ────────────
		services.AddScoped<IUnitOfWork, UnitOfWork>();

		return services;
	}
}