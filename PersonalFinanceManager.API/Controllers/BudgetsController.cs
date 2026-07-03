using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Application.Budgets;
using PersonalFinanceManager.Application.Budgets.Dtos;

namespace PersonalFinanceManager.API.Controllers;

[Authorize]
public class BudgetsController : BaseApiController
{
	private readonly BudgetAppService _budgets;

	public BudgetsController(BudgetAppService budgets) => _budgets = budgets;

	[HttpGet]
	public async Task<ActionResult<IEnumerable<BudgetDto>>> GetAll(CancellationToken ct)
		=> Ok(await _budgets.GetAllAsync(ct));

	[HttpGet("active")]
	public async Task<ActionResult<IEnumerable<BudgetDto>>> GetActive(CancellationToken ct)
		=> Ok(await _budgets.GetActiveAsync(ct));

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<BudgetDto>> GetById(Guid id, CancellationToken ct)
		=> Ok(await _budgets.GetByIdAsync(id, ct));

	[HttpPost]
	public async Task<ActionResult<BudgetDto>> Create(CreateBudgetRequest request, CancellationToken ct)
	{
		var result = await _budgets.CreateAsync(request, ct);
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	[HttpPut("{id:guid}")]
	public async Task<ActionResult<BudgetDto>> Update(Guid id, UpdateBudgetRequest request, CancellationToken ct)
		=> Ok(await _budgets.UpdateAsync(id, request, ct));

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		await _budgets.DeleteAsync(id, ct);
		return NoContent();
	}
}
