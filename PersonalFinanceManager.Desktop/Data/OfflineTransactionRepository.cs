using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Desktop.Data.Entities;

namespace PersonalFinanceManager.Desktop.Data;

public class OfflineTransactionRepository : IOfflineTransactionRepository
{
    private readonly OfflineDbContext _db;

    public OfflineTransactionRepository(OfflineDbContext db) => _db = db;

    public Task<List<OfflineTransaction>> GetAllAsync(CancellationToken ct = default)
        => _db.OfflineTransactions.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);

    public Task<List<OfflineTransaction>> GetUnsynedAsync(CancellationToken ct = default)
        => _db.OfflineTransactions
              .Where(t => !t.IsSynced && !t.SyncFailed)
              .OrderBy(t => t.CreatedAt)
              .ToListAsync(ct);

    public async Task<OfflineTransaction> AddAsync(OfflineTransaction tx, CancellationToken ct = default)
    {
        _db.OfflineTransactions.Add(tx);
        await _db.SaveChangesAsync(ct);
        return tx;
    }

    public async Task MarkSyncedAsync(Guid id, CancellationToken ct = default)
    {
        var tx = await _db.OfflineTransactions.FindAsync(new object[] { id }, ct);
        if (tx is null) return;
        tx.IsSynced = true;
        tx.SyncedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default)
    {
        var tx = await _db.OfflineTransactions.FindAsync(new object[] { id }, ct);
        if (tx is null) return;
        tx.SyncFailed = true;
        tx.SyncError = error;
        await _db.SaveChangesAsync(ct);
    }
}
