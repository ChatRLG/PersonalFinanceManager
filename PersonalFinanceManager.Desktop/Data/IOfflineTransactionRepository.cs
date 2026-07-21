using PersonalFinanceManager.Desktop.Data.Entities;

namespace PersonalFinanceManager.Desktop.Data;

public interface IOfflineTransactionRepository
{
    Task<List<OfflineTransaction>> GetUnsynedAsync(CancellationToken ct = default);
    Task<OfflineTransaction> AddAsync(OfflineTransaction tx, CancellationToken ct = default);
    Task MarkSyncedAsync(Guid id, CancellationToken ct = default);
    Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default);
    Task<List<OfflineTransaction>> GetAllAsync(CancellationToken ct = default);
}
