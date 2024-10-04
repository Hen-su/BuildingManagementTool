using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class CategoryViewModel
    {
        public Property Property { get; set; }
        public IEnumerable<PropertyCategory> PropertyCategories { get; } = new List<PropertyCategory>();
        public ManagePropertyFormViewModel ManagePropertyFormViewModel { get; set; }
        public List<CategoryPreviewViewModel> CategoryPreviewViewModels { get; set; }

        public CategoryViewModel(IEnumerable<PropertyCategory> propertyCategories, ManagePropertyFormViewModel managePropertyFormViewModel, Property property, List<CategoryPreviewViewModel> previewViewModels)
        {
            PropertyCategories = propertyCategories;
            ManagePropertyFormViewModel = managePropertyFormViewModel;
            Property = property;
            CategoryPreviewViewModels = previewViewModels;
        }
    }
}
