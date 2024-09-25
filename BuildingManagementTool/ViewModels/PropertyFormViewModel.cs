using BuildingManagementTool.Models;
using System.ComponentModel.DataAnnotations;

namespace BuildingManagementTool.ViewModels
{
    public class PropertyFormViewModel
    {
        public Property? currentProperty { get; set; }
        [Required(ErrorMessage = "The property name is required")]
        [StringLength(100, ErrorMessage = "Category name must not exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Only upper and lowercase characters, numbers, hyphen, and underscore are allowed.")]
        public string PropertyName { get; set; }

        public PropertyFormViewModel(Property? Property) 
        {
            currentProperty = Property;
        }
    }
}
