using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class CategoryViewModel
    {
        public Property Property { get; set; }
        public IEnumerable<PropertyCategory> PropertyCategories { get; } = new List<PropertyCategory>();
        public string? PropertyImageUrl { get; }
        public CategoryViewModel(IEnumerable<PropertyCategory> propertyCategories, string? propertyImageUrl, Property property) 
        {
            PropertyCategories = propertyCategories;
            PropertyImageUrl = propertyImageUrl;
            Property = property;
        }
    }
}
