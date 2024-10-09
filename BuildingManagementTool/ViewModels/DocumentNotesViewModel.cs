using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class DocumentNotesViewModel
    {
        public IEnumerable<Document> Documents { get; set; }
        public string Role { get; set; }
        public DocumentNotesViewModel(IEnumerable<Document> documents, string role)
        {
            Documents = documents;
            Role = role;
        }
    }
}
