using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class DocumentOptionsViewModel
    {
        public Document Document { get; set; }
        public string Role { get; set; }
        public DocumentOptionsViewModel(Document document, string role) 
        { 
            Document = document;
            Role = role;
        }
    }
}
