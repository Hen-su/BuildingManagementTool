using BuildingManagementTool.Models;
using System.ComponentModel.DataAnnotations;

namespace BuildingManagementTool.ViewModels
{
    public class CategoryFormViewModel
    {
        public IEnumerable<Category> Categories { get; set; }   
        public int CurrentPropertyId { get; set; }
        public PropertyCategory? CurrentCategory { get; set; }
        [Required(ErrorMessage = "The category name is required")]
        [StringLength(50, ErrorMessage = "Category name must not exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Only upper and lowercase characters, numbers, hyphen, and underscore are allowed.")]
        public string CategoryName { get; set; }
        public CategoryFormViewModel(IEnumerable<Category> categories, int currentPropertyId, PropertyCategory? currentCategory) 
        {
            Categories = categories;
            CurrentPropertyId = currentPropertyId;
            CurrentCategory = currentCategory;
        }
    }
}
