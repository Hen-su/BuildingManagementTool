using BuildingManagementTool.Models;
using System.ComponentModel.DataAnnotations;

namespace BuildingManagementTool.ViewModels
{
    public class CategoryFormViewModel
    {
        public IEnumerable<Category> Categories { get; set; }   
        public int CurrentPropertyId { get; set; }
        [Required(ErrorMessage = "The category name is required")]
        [StringLength(50, ErrorMessage = "Category name must not exceed 50 characters")]
        public string CategoryName { get; set; }
        public CategoryFormViewModel(IEnumerable<Category> categories, int currentPropertyId) 
        {
            Categories = categories;
            CurrentPropertyId = currentPropertyId;
        }
    }
}
