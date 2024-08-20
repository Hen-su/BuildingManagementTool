using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class CategoryViewModel
    {
        public IEnumerable<PropertyCategory> Categories { get; }
        public string? PropertyImageUrl { get; }
        public CategoryViewModel(IEnumerable<PropertyCategory> categories, string? propertyImageUrl) 
        {
            Categories = categories;
            PropertyImageUrl = propertyImageUrl;
        }
    }
}
