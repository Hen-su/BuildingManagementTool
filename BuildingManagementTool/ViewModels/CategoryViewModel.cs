using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class CategoryViewModel
    {
        public IEnumerable<PropertyCategory> PropertyCategories { get; } = new List<PropertyCategory>();
        public string? PropertyImageUrl { get; }
        public CategoryViewModel(IEnumerable<PropertyCategory> propertyCategories, string? propertyImageUrl) 
        {
            PropertyCategories = propertyCategories;
            PropertyImageUrl = propertyImageUrl;
        }
    }
}
