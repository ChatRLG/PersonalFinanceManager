using PersonalFinanceManager.Core.Entities;

namespace PersonalFinanceManager.Core.Interfaces;

/// <summary>
/// Generic repository contract for basic CRUD operations.
/// All entity-specific repositories extend this.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
	Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
	Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
	Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
