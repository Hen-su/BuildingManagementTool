using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class PropertyViewModel
    {
        public UserProperty UserProperty { get; set; }
        public string ImageUrl { get; set; }
        public PropertyViewModel(UserProperty userProperty, string url) 
        {
            UserProperty = userProperty;
            ImageUrl = url;
        }
    }
}
