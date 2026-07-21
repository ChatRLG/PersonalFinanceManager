using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Desktop.Data.Entities;

namespace PersonalFinanceManager.Desktop.Data;

/// <summary>
/// EF Core SQLite context for local offline storage.
/// Separate from the API's AppDBContext — manages only Desktop-local data.
/// </summary>
public class OfflineDbContext : DbContext
{
    public OfflineDbContext(DbContextOptions<OfflineDbContext> options) : base(options) { }

    public DbSet<OfflineTransaction> OfflineTransactions => Set<OfflineTransaction>();
    public DbSet<SyncQueueEntry> SyncQueue => Set<SyncQueueEntry>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<OfflineTransaction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasColumnType("TEXT"); // SQLite stores decimal as TEXT
            e.Property(x => x.Currency).HasMaxLength(10);
            e.Property(x => x.Type).HasMaxLength(20);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Notes).HasMaxLength(1000);
        });

        mb.Entity<SyncQueueEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.EntityType).HasMaxLength(50);
            e.Property(x => x.OperationType).HasMaxLength(20);
            e.HasIndex(x => x.SyncedAt); // fast lookup of unsynced entries
        });
    }
}
