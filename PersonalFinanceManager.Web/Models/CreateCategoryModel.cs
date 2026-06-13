using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceManager.Web.Models;

public class CreateCategoryModel
{
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(50, MinimumLength = 2)]
    public string Name{ get; set; } = string.Empty;

    [Required(ErrorMessage = "Type is required.")]
        public string Type{ get; set; } = string.Empty;

    public string ? Icon{ get; set; }
}
