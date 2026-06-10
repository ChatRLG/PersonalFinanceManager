using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Interfaces;
using PersonalFinanceManager.Infrastructure.Data;

namespace PersonalFinanceManager.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation using EF Core.
/// Provides basic CRUD operations for any entity that extends BaseEntity.
/// </summary>
public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
	protected readonly AppDBContext Context;
	protected readonly DbSet<T> DbSet;

	public GenericRepository(AppDBContext context)
	{
		Context = context;
		DbSet = context.Set<T>();
	}

	public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await DbSet.FindAsync(new object[] { id }, cancellationToken);
	}

	public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await DbSet.ToListAsync(cancellationToken);
	}

	public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
	{
		await DbSet.AddAsync(entity, cancellationToken);
		return entity;
	}

	public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
	{
		Context.Entry(entity).State = EntityState.Modified;
		return Task.CompletedTask;
	}

	public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var entity = await GetByIdAsync(id, cancellationToken);
		if (entity is not null)
		{
			entity.MarkAsDeleted();
			Context.Entry(entity).State = EntityState.Modified;
		}
	}

	public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await DbSet.AnyAsync(e => e.Id == id, cancellationToken);
	}
}