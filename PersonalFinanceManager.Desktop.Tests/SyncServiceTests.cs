using FluentAssertions;
using Moq;
using PersonalFinanceManager.Application.Contracts.Transactions;
using PersonalFinanceManager.Desktop.Data;
using PersonalFinanceManager.Desktop.Data.Entities;
using PersonalFinanceManager.Desktop.Services;
using System.Net;
using Xunit;

namespace PersonalFinanceManager.Desktop.Tests;

/// <summary>
/// Unit tests for SyncService.
/// All external dependencies are mocked; no WPF APIs are used.
/// </summary>
public class SyncServiceTests
{
    private static OfflineTransaction MakeTx(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Amount = 50m,
        Currency = "USD",
        Type = "Expense",
        Description = "Test",
        TransactionDate = DateTime.Today,
        AccountId = Guid.NewGuid(),
        CategoryId = Guid.NewGuid()
    };

    [Fact]
    public async Task SyncAsync_WhenOnlineAndPending_PostsTransactionAndMarksSynced()
    {
        var tx = MakeTx();
        var repo = new Mock<IOfflineTransactionRepository>();
        var api = new Mock<IApiClient>();
        var conn = new Mock<IConnectivityService>();

        repo.Setup(r => r.GetUnsynedAsync(default)).ReturnsAsync(new List<OfflineTransaction> { tx });
        api.Setup(a => a.CreateTransactionAsync(It.IsAny<CreateTransactionRequest>(), default))
           .ReturnsAsync(new TransactionDto { Id = Guid.NewGuid(), Amount = tx.Amount });
        conn.Setup(c => c.CheckAsync(default)).ReturnsAsync(true);

        await new SyncService(repo.Object, api.Object, conn.Object).SyncAsync();

        api.Verify(a => a.CreateTransactionAsync(It.IsAny<CreateTransactionRequest>(), default), Times.Once);
        repo.Verify(r => r.MarkSyncedAsync(tx.Id, default), Times.Once);
        repo.Verify(r => r.MarkFailedAsync(It.IsAny<Guid>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task SyncAsync_WhenNoPendingItems_DoesNotCallApi()
    {
        var repo = new Mock<IOfflineTransactionRepository>();
        var api = new Mock<IApiClient>();
        var conn = new Mock<IConnectivityService>();

        repo.Setup(r => r.GetUnsynedAsync(default)).ReturnsAsync(new List<OfflineTransaction>());
        conn.Setup(c => c.CheckAsync(default)).ReturnsAsync(true);

        await new SyncService(repo.Object, api.Object, conn.Object).SyncAsync();

        api.Verify(a => a.CreateTransactionAsync(It.IsAny<CreateTransactionRequest>(), default), Times.Never);
    }

    [Fact]
    public async Task SyncAsync_WhenOffline_SkipsWithoutCallingApi()
    {
        var repo = new Mock<IOfflineTransactionRepository>();
        var api = new Mock<IApiClient>();
        var conn = new Mock<IConnectivityService>();

        conn.Setup(c => c.CheckAsync(default)).ReturnsAsync(false);

        await new SyncService(repo.Object, api.Object, conn.Object).SyncAsync();

        repo.Verify(r => r.GetUnsynedAsync(default), Times.Never);
        api.Verify(a => a.CreateTransactionAsync(It.IsAny<CreateTransactionRequest>(), default), Times.Never);
    }

    [Fact]
    public async Task SyncAsync_When4xxFromApi_MarksFailedDoesNotMarkSynced()
    {
        var tx = MakeTx();
        var repo = new Mock<IOfflineTransactionRepository>();
        var api = new Mock<IApiClient>();
        var conn = new Mock<IConnectivityService>();

        repo.Setup(r => r.GetUnsynedAsync(default)).ReturnsAsync(new List<OfflineTransaction> { tx });
        api.Setup(a => a.CreateTransactionAsync(It.IsAny<CreateTransactionRequest>(), default))
           .ThrowsAsync(new HttpRequestException("Bad Request", null, HttpStatusCode.BadRequest));
        conn.Setup(c => c.CheckAsync(default)).ReturnsAsync(true);

        await new SyncService(repo.Object, api.Object, conn.Object).SyncAsync();

        repo.Verify(r => r.MarkFailedAsync(tx.Id, It.IsAny<string>(), default), Times.Once);
        repo.Verify(r => r.MarkSyncedAsync(tx.Id, default), Times.Never);
    }

    [Fact]
    public async Task SyncAsync_WhenNetworkError_LeavesItemUntouched()
    {
        var tx = MakeTx();
        var repo = new Mock<IOfflineTransactionRepository>();
        var api = new Mock<IApiClient>();
        var conn = new Mock<IConnectivityService>();

        repo.Setup(r => r.GetUnsynedAsync(default)).ReturnsAsync(new List<OfflineTransaction> { tx });
        // Network error — no status code → not treated as 4xx
        api.Setup(a => a.CreateTransactionAsync(It.IsAny<CreateTransactionRequest>(), default))
           .ThrowsAsync(new HttpRequestException("Network unreachable"));
        conn.Setup(c => c.CheckAsync(default)).ReturnsAsync(true);

        await new SyncService(repo.Object, api.Object, conn.Object).SyncAsync();

        // Leave for retry — neither synced nor failed
        repo.Verify(r => r.MarkSyncedAsync(tx.Id, default), Times.Never);
        repo.Verify(r => r.MarkFailedAsync(tx.Id, It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task SyncAsync_WithMultiplePending_SyncsAll()
    {
        var tx1 = MakeTx();
        var tx2 = MakeTx();
        var synced = new List<Guid>();

        var repo = new Mock<IOfflineTransactionRepository>();
        var api = new Mock<IApiClient>();
        var conn = new Mock<IConnectivityService>();

        repo.Setup(r => r.GetUnsynedAsync(default))
            .ReturnsAsync(new List<OfflineTransaction> { tx1, tx2 });
        api.Setup(a => a.CreateTransactionAsync(It.IsAny<CreateTransactionRequest>(), default))
           .ReturnsAsync(new TransactionDto { Id = Guid.NewGuid() });
        repo.Setup(r => r.MarkSyncedAsync(It.IsAny<Guid>(), default))
            .Callback<Guid, CancellationToken>((id, _) => synced.Add(id))
            .Returns(Task.CompletedTask);
        conn.Setup(c => c.CheckAsync(default)).ReturnsAsync(true);

        await new SyncService(repo.Object, api.Object, conn.Object).SyncAsync();

        synced.Should().Contain(tx1.Id).And.Contain(tx2.Id);
        api.Verify(a => a.CreateTransactionAsync(It.IsAny<CreateTransactionRequest>(), default), Times.Exactly(2));
    }
}
