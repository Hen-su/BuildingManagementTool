using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class CategoryViewModel
    {
        public Property Property { get; set; }
        public IEnumerable<PropertyCategory> PropertyCategories { get; } = new List<PropertyCategory>();
        public List<Dictionary<int, List<string>>> ImageList { get; set; }
        public List<CategoryPreviewViewModel> CategoryPreviewViewModels { get; set; }
        public string Role {  get; set; }
        
        public CategoryViewModel(IEnumerable<PropertyCategory> propertyCategories, List<Dictionary<int, List<string>>> imageList, Property property, List<CategoryPreviewViewModel> previewViewModels, string role) 
        {
            PropertyCategories = propertyCategories;
            ImageList = imageList;
            Property = property;
            CategoryPreviewViewModels = previewViewModels;
            Role = role;
        }
    }
}
