using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class DocumentOptionsViewModel
    {
        public Document Document { get; set; }
        public PropertyCategory CurrentCategory { get; set; }
        public DocumentOptionsViewModel(Document document, PropertyCategory propertyCategory) 
        { 
            Document = document;
            CurrentCategory = propertyCategory;
        }
    }
}
