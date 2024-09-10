using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class CategoryViewModel
    {
        public IEnumerable<PropertyCategory> PropertyCategories { get; set; }
        public string PropertyImageUrl { get; set; }

        // Add a collection of documents for each category
        public Dictionary<int, List<Document>> CategoryDocuments { get; set; }

        public CategoryViewModel(IEnumerable<PropertyCategory> propertyCategories, string propertyImageUrl)
        {
            PropertyCategories = propertyCategories;
            PropertyImageUrl = propertyImageUrl;
            CategoryDocuments = new Dictionary<int, List<Document>>();
        }
    }
}
