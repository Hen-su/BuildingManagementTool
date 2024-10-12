using System.ComponentModel.DataAnnotations;

namespace BuildingManagementTool.ViewModels
{
    public class AddViewerViewModel
    {
        [Required(ErrorMessage = "Email cannot be empty")]
        [EmailAddress(ErrorMessage = "Please enter a valid email")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; }
        public AddViewerViewModel() { }
    }
}
