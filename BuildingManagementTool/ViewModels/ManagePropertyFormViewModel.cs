using BuildingManagementTool.Models;
using BuildingManagementTool.Models.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace BuildingManagementTool.ViewModels
{
    public class ManagePropertyFormViewModel
    {
        [ValidateNever]
        public List<Dictionary<int, List<string>>>? ImageUrls { get; set; } = new List<Dictionary<int, List<string>>>();
        [ValidateNever]
        public Property? CurrentProperty { get; set; } = new Property();
        [Required(ErrorMessage = "The property name cannot be empty")]
        [StringLength(100, ErrorMessage = "Property name must not exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9 _-]+$", ErrorMessage = "Only upper and lowercase alphanumeric characters, hyphens, underscores and spaces are allowed.")]
        public string PropertyName { get; set; }
        public List<IFormFile>? Images { get; set; }
        [BindNever]
        [ValidateNever]
        public Dictionary<int, string> ViewerEmails { get; set; } = new Dictionary<int, string>();
        [ValidateNever]
        public AddViewerViewModel AddViewerViewModel { get; set; }
        public ManagePropertyFormViewModel(List<Dictionary<int, List<string>>> images, Property property, string propertyName, Dictionary<int, string> emails) 
        {
            ImageUrls = images;
            CurrentProperty = property;
            PropertyName = propertyName;
            ViewerEmails = emails;
        }
        public ManagePropertyFormViewModel() { }
    }
}
