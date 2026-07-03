using PersonalFinanceManager.Core.Entities;

namespace PersonalFinanceManager.Application.Categories.Dtos;

/// <summary>Response shape for a category. Matches the Web CategoryDto.</summary>
public class CategoryDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public string? Icon { get; set; }
	public string? Colour { get; set; }
	public DateTime CreatedAt { get; set; }

	public static CategoryDto FromEntity(Category c) => new()
	{
		Id = c.Id,
		Name = c.Name,
		Type = c.Type.ToString(),
		Icon = c.Icon,
		Colour = c.Colour,
		CreatedAt = c.CreatedAt
	};
}
