using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalFinanceManager.Desktop.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "OfflineTransactions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                Currency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                Type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                TransactionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                CategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                DestinationAccountId = table.Column<Guid>(type: "TEXT", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                IsSynced = table.Column<bool>(type: "INTEGER", nullable: false),
                SyncedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                SyncFailed = table.Column<bool>(type: "INTEGER", nullable: false),
                SyncError = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OfflineTransactions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SyncQueue",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                EntityType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                EntityId = table.Column<Guid>(type: "TEXT", nullable: true),
                LocalEntityId = table.Column<Guid>(type: "TEXT", nullable: true),
                OperationType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                Payload = table.Column<string>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                SyncedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                SyncFailed = table.Column<bool>(type: "INTEGER", nullable: false),
                SyncError = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SyncQueue", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SyncQueue_SyncedAt",
            table: "SyncQueue",
            column: "SyncedAt");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "OfflineTransactions");
        migrationBuilder.DropTable(name: "SyncQueue");
    }
}
