using PersonalFinanceManager.Application.Budgets.Dtos;
using PersonalFinanceManager.Application.Common.Interfaces;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Exceptions;
using PersonalFinanceManager.Core.Interfaces;

namespace PersonalFinanceManager.Application.Budgets;

public class BudgetAppService
{
	private readonly IUnitOfWork _uow;
	private readonly ICurrentUser _currentUser;

	public BudgetAppService(IUnitOfWork uow, ICurrentUser currentUser)
	{
		_uow = uow;
		_currentUser = currentUser;
	}

	public async Task<IEnumerable<BudgetDto>> GetAllAsync(CancellationToken ct = default)
	{
		var userId = RequireUserId();
		var budgets = await _uow.Budgets.GetByUserIdAsync(userId, ct);
		return budgets.Select(b => BudgetDto.FromEntity(b));
	}

	public async Task<IEnumerable<BudgetDto>> GetActiveAsync(CancellationToken ct = default)
	{
		var userId = RequireUserId();
		var budgets = await _uow.Budgets.GetActiveBudgetsAsync(userId, ct);
		return budgets.Select(b => BudgetDto.FromEntity(b));
	}

	public async Task<BudgetDto> GetByIdAsync(Guid id, CancellationToken ct = default)
	{
		var budget = await RequireOwnedBudgetAsync(id, ct);
		return BudgetDto.FromEntity(budget);
	}

	public async Task<BudgetDto> CreateAsync(CreateBudgetRequest request, CancellationToken ct = default)
	{
		var userId = RequireUserId();

		if (request.EndDate <= request.StartDate)
			throw new ArgumentException("End date must be after start date.");

		// Load full profile so the User aggregate can enforce overlap/ownership rules.
		var user = await _uow.Users.GetWithFullProfileAsync(userId, ct)
			?? throw new EntityNotFoundException(nameof(User), userId);

		var budget = user.AddBudget(
			request.Name, request.Limit, request.Currency,
			request.Period, request.StartDate, request.EndDate, request.CategoryId);

		await _uow.Budgets.AddAsync(budget, ct);
		await _uow.SaveChangesAsync(ct);

		// Reload with category navigation.
		var saved = await _uow.Budgets.GetWithCategoryAsync(budget.Id, ct) ?? budget;
		return BudgetDto.FromEntity(saved);
	}

	public async Task<BudgetDto> UpdateAsync(Guid id, UpdateBudgetRequest request, CancellationToken ct = default)
	{
		var budget = await RequireOwnedBudgetAsync(id, ct);

		budget.UpdateLimit(request.Limit);
		budget.UpdateDetails(request.Name, request.StartDate, request.EndDate);

		await _uow.Budgets.UpdateAsync(budget, ct);
		await _uow.SaveChangesAsync(ct);

		var saved = await _uow.Budgets.GetWithCategoryAsync(budget.Id, ct) ?? budget;
		return BudgetDto.FromEntity(saved);
	}

	public async Task DeleteAsync(Guid id, CancellationToken ct = default)
	{
		var budget = await RequireOwnedBudgetAsync(id, ct);
		await _uow.Budgets.DeleteAsync(budget.Id, ct);
		await _uow.SaveChangesAsync(ct);
	}

	// ── Helpers ──────────────────────────────────────────────────────────

	private Guid RequireUserId() =>
		_currentUser.UserId ?? throw new UnauthorizedException("User identity could not be resolved.");

	private async Task<Budget> RequireOwnedBudgetAsync(Guid id, CancellationToken ct)
	{
		var budget = await _uow.Budgets.GetWithCategoryAsync(id, ct)
			?? throw new EntityNotFoundException(nameof(Budget), id);

		if (budget.UserId != RequireUserId())
			throw new EntityNotFoundException(nameof(Budget), id);

		return budget;
	}
}
