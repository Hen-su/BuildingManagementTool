using BuildingManagementTool.Models;

namespace BuildingManagementTool.ViewModels
{
    public class PropertyViewModel
    {
        public UserProperty UserProperty { get; set; }
        public string ImageUrl { get; set; }
        public string Role {  get; set; }
        public PropertyViewModel(UserProperty userProperty, string url, string role) 
        {
            UserProperty = userProperty;
            ImageUrl = url;
            Role = role;
        }
    }
}
