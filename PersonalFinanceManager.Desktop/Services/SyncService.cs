using System.Net.Http;
using System.Text.Json;
using PersonalFinanceManager.Application.Contracts.Transactions;
using PersonalFinanceManager.Desktop.Data;

namespace PersonalFinanceManager.Desktop.Services;

/// <summary>
/// Syncs pending offline transactions to the API.
///
/// Policy:
///   - Walk <see cref="IOfflineTransactionRepository.GetUnsynedAsync"/> in CreatedAt order.
///   - POST each to the API.
///   - 2xx → mark synced (SyncedAt = now).
///   - 4xx validation/conflict → mark SyncFailed = true, store error; no retry.
///   - Network error / 5xx → leave unsynchronised; retry on next cycle.
///
/// Conflict resolution: last-write-wins by server UpdatedAt (server is source of truth).
/// For offline creates, duplicate detection relies on idempotency via LocalEntityId checked
/// in future iterations.
/// </summary>
public class SyncService : ISyncService
{
    private readonly IOfflineTransactionRepository _repo;
    private readonly IApiClient _api;
    private readonly IConnectivityService _connectivity;

    public SyncService(
        IOfflineTransactionRepository repo,
        IApiClient api,
        IConnectivityService connectivity)
    {
        _repo = repo;
        _api = api;
        _connectivity = connectivity;
    }

    public async Task SyncAsync(CancellationToken ct = default)
    {
        var online = await _connectivity.CheckAsync(ct);
        if (!online) return;

        var pending = await _repo.GetUnsynedAsync(ct);
        if (pending.Count == 0) return;

        foreach (var tx in pending)
        {
            ct.ThrowIfCancellationRequested();

            var req = new CreateTransactionRequest
            {
                Amount = tx.Amount,
                Currency = tx.Currency,
                Type = tx.Type,
                Description = tx.Description,
                Notes = tx.Notes,
                TransactionDate = tx.TransactionDate,
                AccountId = tx.AccountId,
                CategoryId = tx.CategoryId,
                DestinationAccountId = tx.DestinationAccountId
            };

            try
            {
                await _api.CreateTransactionAsync(req, ct);
                await _repo.MarkSyncedAsync(tx.Id, ct);
            }
            catch (HttpRequestException httpEx) when (IsClientError(httpEx))
            {
                // 4xx: permanent failure — mark failed and don't retry.
                await _repo.MarkFailedAsync(tx.Id, $"4xx: {httpEx.Message}", ct);
            }
            catch (Exception)
            {
                // Network error / 5xx — leave for next cycle; don't mark failed.
            }
        }
    }

    private static bool IsClientError(HttpRequestException ex)
        => ex.StatusCode.HasValue &&
           (int)ex.StatusCode.Value is >= 400 and < 500;
}
