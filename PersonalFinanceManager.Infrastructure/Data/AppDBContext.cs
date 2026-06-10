using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Core.Entities;

namespace PersonalFinanceManager.Infrastructure.Data;

/// <summary>
/// The main EF Core database context for the application.
/// Handles entity mapping, soft-delete query filters, and audit fields.
/// </summary>
public class AppDBContext : DbContext
{
	public DbSet<User> Users => Set<User>();

	public DbSet<Account> Accounts => Set<Account>();

	//public DbSet<Transaction> Transactions => Set<Transaction>();
	public DbSet<Category> Categories => Set<Category>();

	public DbSet<Budget> Budgets => Set<Budget>();

	public DbSet<PersonalFinanceManager.Core.Entities.Transaction> Transactions => Set<PersonalFinanceManager.Core.Entities.Transaction>();

	public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Apply all IEntityTypeConfiguration classes from this assembly
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDBContext).Assembly);

		// Global query filter: automatically exclude soft-deleted entities
		modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
		modelBuilder.Entity<Account>().HasQueryFilter(e => !e.IsDeleted);
		modelBuilder.Entity<PersonalFinanceManager.Core.Entities.Transaction>().HasQueryFilter(e => !e.IsDeleted);
		modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
		modelBuilder.Entity<Budget>().HasQueryFilter(e => !e.IsDeleted);
	}

	/// <summary>
	/// Automatically populates CreatedAt / UpdatedAt before saving.
	/// </summary>
	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;

		foreach (var entry in ChangeTracker.Entries<BaseEntity>())
		{
			switch (entry.State)
			{
				case EntityState.Added:
					entry.Entity.CreatedAt = now;
					break;

				case EntityState.Modified:
					entry.Entity.UpdatedAt = now;
					// Prevent overwriting CreatedAt on update
					entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
					break;
			}
		}

		return await base.SaveChangesAsync(cancellationToken);
	}
}
