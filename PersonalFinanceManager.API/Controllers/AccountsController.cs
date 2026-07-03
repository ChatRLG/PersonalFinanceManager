using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Application.Accounts;
using PersonalFinanceManager.Application.Accounts.Dtos;

namespace PersonalFinanceManager.API.Controllers;

[Authorize]
public class AccountsController : BaseApiController
{
	private readonly AccountAppService _accounts;

	public AccountsController(AccountAppService accounts) => _accounts = accounts;

	[HttpGet]
	public async Task<ActionResult<IEnumerable<AccountDto>>> GetAll(CancellationToken ct)
		=> Ok(await _accounts.GetAllAsync(ct));

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<AccountDto>> GetById(Guid id, CancellationToken ct)
		=> Ok(await _accounts.GetByIdAsync(id, ct));

	[HttpPost]
	public async Task<ActionResult<AccountDto>> Create(CreateAccountRequest request, CancellationToken ct)
	{
		var result = await _accounts.CreateAsync(request, ct);
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	[HttpPut("{id:guid}")]
	public async Task<ActionResult<AccountDto>> Update(Guid id, UpdateAccountRequest request, CancellationToken ct)
		=> Ok(await _accounts.UpdateAsync(id, request, ct));

	[HttpPut("{id:guid}/activate")]
	public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
	{
		await _accounts.ActivateAsync(id, ct);
		return NoContent();
	}

	[HttpPut("{id:guid}/deactivate")]
	public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
	{
		await _accounts.DeactivateAsync(id, ct);
		return NoContent();
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		await _accounts.DeleteAsync(id, ct);
		return NoContent();
	}
}
