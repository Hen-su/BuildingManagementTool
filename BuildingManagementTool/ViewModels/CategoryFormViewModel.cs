using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace BuildingManagementTool.ViewModels
{
    public class CategoryFormViewModel
    {
        [BindNever]
        [ValidateNever]
        public IEnumerable<Category> Categories { get; set; }
        [BindNever]
        [ValidateNever]
        public int CurrentPropertyId { get; set; }
        [BindNever]
        [ValidateNever]
        public PropertyCategory? CurrentCategory { get; set; }
        [Required(ErrorMessage = "The category name is required")]
        [StringLength(50, ErrorMessage = "Category name must not exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9 _-]+$", ErrorMessage = "Only upper and lowercase characters, spaces, hyphen, and underscore are allowed.")]
        public string CategoryName { get; set; }
        public CategoryFormViewModel(IEnumerable<Category> categories, int currentPropertyId, PropertyCategory? currentCategory) 
        {
            Categories = categories;
            CurrentPropertyId = currentPropertyId;
            CurrentCategory = currentCategory;
        }
        public CategoryFormViewModel() { }
    }
}
