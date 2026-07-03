using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceManager.Application.Categories;
using PersonalFinanceManager.Application.Categories.Dtos;
using PersonalFinanceManager.Core.Enums;

namespace PersonalFinanceManager.API.Controllers;

[Authorize]
public class CategoriesController : BaseApiController
{
	private readonly CategoryAppService _categories;

	public CategoriesController(CategoryAppService categories) => _categories = categories;

	/// <summary>Returns all categories. Filter by type with ?type=Expense or ?type=Income.</summary>
	[HttpGet]
	public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll(
		[FromQuery] TransactionType? type, CancellationToken ct)
		=> Ok(await _categories.GetAllAsync(type, ct));

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken ct)
		=> Ok(await _categories.GetByIdAsync(id, ct));

	[HttpPost]
	public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request, CancellationToken ct)
	{
		var result = await _categories.CreateAsync(request, ct);
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		await _categories.DeleteAsync(id, ct);
		return NoContent();
	}
}
