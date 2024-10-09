using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class CategoryViewModel
    {
        public Property Property { get; set; }
        public IEnumerable<PropertyCategory> PropertyCategories { get; } = new List<PropertyCategory>();
        public string? PropertyImageUrl { get; }
        public List<CategoryPreviewViewModel> CategoryPreviewViewModels { get; set; }
        public string Role {  get; set; }
        
        public CategoryViewModel(IEnumerable<PropertyCategory> propertyCategories, string? propertyImageUrl, Property property, List<CategoryPreviewViewModel> previewViewModels, string role) 
        {
            PropertyCategories = propertyCategories;
            PropertyImageUrl = propertyImageUrl;
            Property = property;
            CategoryPreviewViewModels = previewViewModels;
            Role = role;
        }
    }
}
