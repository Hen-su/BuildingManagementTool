using BuildingManagementTool.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace BuildingManagementTool.ViewModels
{
    public class AddNoteViewModel
    {
        [BindNever]
        [ValidateNever]
        public Document Document { get; set; }
        [Required(ErrorMessage = "Note cannot be empty")]
        [StringLength(50, ErrorMessage = "Note cannot exceed 50 characters")]
        public string Note { get; set; }
        public AddNoteViewModel() { }
    }
}
