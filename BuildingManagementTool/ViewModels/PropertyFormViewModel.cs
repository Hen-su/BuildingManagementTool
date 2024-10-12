using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace BuildingManagementTool.ViewModels
{
    public class PropertyFormViewModel
    {
        [ValidateNever]
        public Property? currentProperty { get; set; } = new Property();
        [Required(ErrorMessage = "The property name is required")]
        [StringLength(100, ErrorMessage = "Property name must not exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9 _-]+$", ErrorMessage = "Only upper and lowercase alphanumeric characters, hyphens, underscores and spaces are allowed.")]
        public string PropertyName { get; set; }

        public PropertyFormViewModel(Property? Property) 
        {
            currentProperty = Property;
        }
        public PropertyFormViewModel() { }
    }
}
