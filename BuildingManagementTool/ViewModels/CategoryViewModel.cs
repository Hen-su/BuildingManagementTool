using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class CategoryViewModel
    {
        public Property Property { get; set; }
        public IEnumerable<PropertyCategory> PropertyCategories { get; } = new List<PropertyCategory>();
        public ManagePropertyFormViewModel ManagePropertyFormViewModel { get; set; }
        public List<CategoryPreviewViewModel> CategoryPreviewViewModels { get; set; }
        public string Role {  get; set; }
        
        public CategoryViewModel(IEnumerable<PropertyCategory> propertyCategories, string? propertyImageUrl, Property property, List<CategoryPreviewViewModel> previewViewModels, string role) 
        {
            PropertyCategories = propertyCategories;
            ManagePropertyFormViewModel = managePropertyFormViewModel;
            Property = property;
            CategoryPreviewViewModels = previewViewModels;
            Role = role;
        }
    }
}
