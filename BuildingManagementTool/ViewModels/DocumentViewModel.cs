using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class DocumentViewModel
    {
        public IEnumerable<Document>? Documents { get; set; }
        public PropertyCategory CurrentCategory { get; set; }
        public string Role {  get; set; }
        public DocumentViewModel(IEnumerable<Document> documents, PropertyCategory currentCategory, string role) 
        {
            Documents = documents;
            CurrentCategory = currentCategory;
            Role = role;
        }
    }
}
