using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Application.Transactions;
using PersonalFinanceManager.Application.Transactions.Dtos;

namespace PersonalFinanceManager.API.Controllers;

[Authorize]
public class TransactionsController : BaseApiController
{
	private readonly TransactionAppService _transactions;

	public TransactionsController(TransactionAppService transactions) => _transactions = transactions;

	/// <summary>
	/// Returns a paginated list of transactions.
	/// Optionally filter by ?accountId=... Default page=1, pageSize=20.
	/// </summary>
	[HttpGet]
	public async Task<ActionResult<PagedResult<TransactionDto>>> GetAll(
		[FromQuery] Guid? accountId,
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 20,
		CancellationToken ct = default)
		=> Ok(await _transactions.GetPaginatedAsync(accountId, page, pageSize, ct));

	/// <summary>Returns all transactions for a specific account (unpaged).</summary>
	[HttpGet("account/{accountId:guid}")]
	public async Task<ActionResult<IEnumerable<TransactionDto>>> GetByAccount(Guid accountId, CancellationToken ct)
		=> Ok(await _transactions.GetByAccountAsync(accountId, ct));

	/// <summary>Returns the most recent N transactions across all user accounts.</summary>
	[HttpGet("recent")]
	public async Task<ActionResult<IEnumerable<TransactionDto>>> GetRecent(
		[FromQuery] int count = 10, CancellationToken ct = default)
		=> Ok(await _transactions.GetRecentAsync(count, ct));

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<TransactionDto>> GetById(Guid id, CancellationToken ct)
		=> Ok(await _transactions.GetByIdAsync(id, ct));

	[HttpPost]
	public async Task<ActionResult<TransactionDto>> Create(CreateTransactionRequest request, CancellationToken ct)
	{
		var result = await _transactions.CreateAsync(request, ct);
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		await _transactions.DeleteAsync(id, ct);
		return NoContent();
	}
}
