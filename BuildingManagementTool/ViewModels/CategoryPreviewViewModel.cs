using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class CategoryPreviewViewModel
    {
        public PropertyCategory PropertyCategory { get; set; }
        // Add a collection of documents for each category
        public Dictionary<int, List<Document>> CategoryDocuments { get; set; }
        public CategoryPreviewViewModel(PropertyCategory propertyCategory, Dictionary<int, List<Document>> categoryDocuments)
        {
            PropertyCategory = propertyCategory;
            CategoryDocuments = categoryDocuments;
        }
        
    }
}
