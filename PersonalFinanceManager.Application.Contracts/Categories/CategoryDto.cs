namespace PersonalFinanceManager.Application.Contracts.Categories;

/// <summary>Response shape for a category. Shared between Web, Desktop, and future clients.</summary>
public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Colour { get; set; }
    public DateTime CreatedAt { get; set; }
}
