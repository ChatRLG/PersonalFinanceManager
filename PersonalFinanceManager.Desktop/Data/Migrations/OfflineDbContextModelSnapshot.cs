using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PersonalFinanceManager.Desktop.Data;

namespace PersonalFinanceManager.Desktop.Data.Migrations;

[DbContext(typeof(OfflineDbContext))]
partial class OfflineDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "8.0.5");

        modelBuilder.Entity("PersonalFinanceManager.Desktop.Data.Entities.OfflineTransaction", b =>
        {
            b.Property<Guid>("Id").HasColumnType("TEXT");
            b.Property<decimal>("Amount").HasColumnType("TEXT");
            b.Property<string>("Currency").IsRequired().HasMaxLength(10).HasColumnType("TEXT");
            b.Property<string>("Type").IsRequired().HasMaxLength(20).HasColumnType("TEXT");
            b.Property<string>("Description").IsRequired().HasMaxLength(500).HasColumnType("TEXT");
            b.Property<string?>("Notes").HasMaxLength(1000).HasColumnType("TEXT");
            b.Property<DateTime>("TransactionDate").HasColumnType("TEXT");
            b.Property<Guid>("AccountId").HasColumnType("TEXT");
            b.Property<Guid>("CategoryId").HasColumnType("TEXT");
            b.Property<Guid?>("DestinationAccountId").HasColumnType("TEXT");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<bool>("IsSynced").HasColumnType("INTEGER");
            b.Property<DateTime?>("SyncedAt").HasColumnType("TEXT");
            b.Property<bool>("SyncFailed").HasColumnType("INTEGER");
            b.Property<string?>("SyncError").HasColumnType("TEXT");
            b.HasKey("Id");
            b.ToTable("OfflineTransactions");
        });

        modelBuilder.Entity("PersonalFinanceManager.Desktop.Data.Entities.SyncQueueEntry", b =>
        {
            b.Property<Guid>("Id").HasColumnType("TEXT");
            b.Property<string>("EntityType").IsRequired().HasMaxLength(50).HasColumnType("TEXT");
            b.Property<Guid?>("EntityId").HasColumnType("TEXT");
            b.Property<Guid?>("LocalEntityId").HasColumnType("TEXT");
            b.Property<string>("OperationType").IsRequired().HasMaxLength(20).HasColumnType("TEXT");
            b.Property<string>("Payload").IsRequired().HasColumnType("TEXT");
            b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            b.Property<DateTime?>("SyncedAt").HasColumnType("TEXT");
            b.Property<bool>("SyncFailed").HasColumnType("INTEGER");
            b.Property<string?>("SyncError").HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("SyncedAt");
            b.ToTable("SyncQueue");
        });
#pragma warning restore 612, 618
    }
}
