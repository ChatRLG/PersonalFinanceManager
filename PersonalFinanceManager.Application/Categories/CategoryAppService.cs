using PersonalFinanceManager.Application.Categories.Dtos;
using PersonalFinanceManager.Application.Common.Interfaces;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.Exceptions;
using PersonalFinanceManager.Core.Interfaces;

namespace PersonalFinanceManager.Application.Categories;

public class CategoryAppService
{
	private readonly IUnitOfWork _uow;
	private readonly ICurrentUser _currentUser;

	public CategoryAppService(IUnitOfWork uow, ICurrentUser currentUser)
	{
		_uow = uow;
		_currentUser = currentUser;
	}

	public async Task<IEnumerable<CategoryDto>> GetAllAsync(TransactionType? type = null, CancellationToken ct = default)
	{
		var userId = RequireUserId();
		var categories = type.HasValue
			? await _uow.Categories.GetByTypeAsync(userId, type.Value, ct)
			: await _uow.Categories.GetByUserIdAsync(userId, ct);
		return categories.Select(CategoryDto.FromEntity);
	}

	public async Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken ct = default)
	{
		var category = await RequireOwnedCategoryAsync(id, ct);
		return CategoryDto.FromEntity(category);
	}

	public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
	{
		var userId = RequireUserId();

		var user = await _uow.Users.GetWithFullProfileAsync(userId, ct)
			?? throw new EntityNotFoundException(nameof(Core.Entities.User), userId);

		var category = user.AddCategory(request.Name, request.Type, request.Icon);

		if (request.Colour is not null)
			category.UpdateDetails(category.Name, request.Icon, request.Colour);

		await _uow.Categories.AddAsync(category, ct);
		await _uow.SaveChangesAsync(ct);
		return CategoryDto.FromEntity(category);
	}

	public async Task DeleteAsync(Guid id, CancellationToken ct = default)
	{
		var category = await RequireOwnedCategoryAsync(id, ct);
		await _uow.Categories.DeleteAsync(category.Id, ct);
		await _uow.SaveChangesAsync(ct);
	}

	// ── Helpers ──────────────────────────────────────────────────────────

	private Guid RequireUserId() =>
		_currentUser.UserId ?? throw new UnauthorizedException("User identity could not be resolved.");

	private async Task<Core.Entities.Category> RequireOwnedCategoryAsync(Guid id, CancellationToken ct)
	{
		var category = await _uow.Categories.GetByIdAsync(id, ct)
			?? throw new EntityNotFoundException(nameof(Core.Entities.Category), id);

		if (category.UserId != RequireUserId())
			throw new EntityNotFoundException(nameof(Core.Entities.Category), id);

		return category;
	}
}
