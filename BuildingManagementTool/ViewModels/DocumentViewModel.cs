using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class DocumentViewModel
    {
        public IEnumerable<Document>? Documents { get; set; }
        public PropertyCategory CurrentCategory { get; set; }

        public DocumentViewModel(IEnumerable<Document> documents, PropertyCategory currentCategory) 
        {
            Documents = documents;
            CurrentCategory = currentCategory;
        }
    }
}
