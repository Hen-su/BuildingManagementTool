using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class CategoryPreviewViewModel
    {
        public PropertyCategory PropertyCategory { get; set; }
        // Add a collection of documents for each category
        public Dictionary<int, List<Document>> CategoryDocuments { get; set; }
        public int CategoryDocumentCount { get; set; }
        public string Role { get; set; }
        public CategoryPreviewViewModel(PropertyCategory propertyCategory, Dictionary<int, List<Document>> categoryDocuments, int count, string role)
        {
            PropertyCategory = propertyCategory;
            CategoryDocuments = categoryDocuments;
            CategoryDocumentCount = count;
            Role = role;
        }
        
    }
}
